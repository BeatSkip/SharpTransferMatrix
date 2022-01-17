using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace SharpTransferMatrix
{
	public class TransferMatrix
	{
		private double frequency;
		private double angle;
		private double k0;
		private Complex kx = new Complex();
		private bool runSetup;
		private List<Layer> structure = new List<Layer>();
		// S parameters
		private Complex rs = 0;
		private Complex rp = 0;
		private Complex ts = 0;
		private Complex tp = 0;

		public TransferMatrix(double frequency, double angle, List<Layer> structure_)
		{
			this.frequency = frequency;
			this.angle = angle;
			this.structure = structure_;
			runSetup = false;
		}
		public TransferMatrix(double frequency, double angle)
		{
			this.frequency = frequency;
			this.angle = angle;
			runSetup = false;
		}

		//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
		public void calculate()
		{

			// Step 1: for a given frequency and angle of incidence 
			// set kz for all layers
			if (structure.Count > 0 && frequency > 0)
			{
				k0 = frequency / Constants.c_const;


				Matrix<Complex> Ms = DenseMatrix.OfArray(new Complex[,] {
					{  1.0,0.0 },
					{ 0.0,1.0 },
				});

				Matrix<Complex> Mp = DenseMatrix.OfArray(new Complex[,] {
					{  1.0,0.0 },
					{ 0.0,1.0 },
				});

			}
			else
			{
				throw new System.Exception("Frequency must be positive and structure array must not be empty.");
			}

			kx = k0 * Math.Sin(angle);

			int layercnt = 0;

			foreach (Layer l in structure)
			{
				Complex kz = Complex.Sqrt(l.getMaterial().getN() * k0 * k0 - kx * kx);

				l.setKz(kz);
				Console.WriteLine($"Layer {layercnt++}:\r");
			}


			// Step 2: calculate transfer matrix
			Ms = interfaceMatrix_s(structure[0], structure[1]);
			Mp = interfaceMatrix_p(structure[0], structure[1]);


			for (int i = 1; i < structure.Count - 1; ++i)
			{

				var propagMatrix = propagate(structure[i], structure[i].getThickness());
				var matrixS = interfaceMatrix_s(structure[i], structure[i + 1]);
				var matrixP = interfaceMatrix_p(structure[i], structure[i + 1]);

				Ms = Ms.Multiply(propagMatrix);
				Mp = Mp.Multiply(propagMatrix);
				//Ms = Matrices.matmul(Ms, propagMatrix);
				//Mp = Matrices.matmul(Mp, propagMatrix);

				//Ms = Matrices.matmul(Ms, matrixS);
				//Mp = Matrices.matmul(Mp, matrixP);
				Ms = Ms.Multiply(matrixS);
				Mp = Mp.Multiply(matrixP);



			}


			// step 3: populate S params 
			rs = Ms[1, 0] / Ms[0, 0];
			rp = Mp[1, 0] / Mp[0, 0];
			ts = 1.0 / Ms[0, 0];
			tp = 1.0 / Mp[0, 0];
			runSetup = true;

		}

		// get S-parameters
		public Complex getRs()
		{
			return rs;
		}
		public Complex getRp()
		{
			return rp;
		}
		public Complex getTs()
		{
			return ts;
		}
		public Complex getTp()
		{
			return tp;
		}
		public List<Complex> getSparams()
		{
			return new List<Complex>() { rs, ts, rp, tp };
		} // get all the above

		// Composite matrices
		public Matrix<Complex> Ms; // s polarization 2x2 matrix
		public Matrix<Complex> Mp; // p polarization 2x2 matrix

		// Elementary interface matrices
		//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
		public Matrix<Complex> interfaceMatrix_s(Layer layer1, Layer layer2)
		{
			Complex eps1 = layer1.getMaterial().getEpsilon();
			Complex mu1 = layer1.getMaterial().getMu();
			Complex k1 = Complex.Sqrt(eps1 * mu1) * k0;

			Complex k1z = Complex.Sqrt(k1.Norm() - kx * kx); // norm(z) = ||z||^2

			Complex eps2 = layer2.getMaterial().getEpsilon();
			Complex mu2 = layer2.getMaterial().getMu();
			Complex k2 = Complex.Sqrt(eps2 * mu2) * k0;
			Complex k2z = Complex.Sqrt(k2.Norm() - kx * kx); // norm(z) = ||z||^2

			Complex normImpedance = k2z * mu1 / (k1z * mu2); // normalized impedance

			Complex M11 = 0.5 + 0.5 * normImpedance;
			Complex M12 = 0.5 - 0.5 * normImpedance;
			Complex M21 = 0.5 - 0.5 * normImpedance;
			Complex M22 = 0.5 + 0.5 * normImpedance;



			Matrix<Complex> Ms = DenseMatrix.OfArray(new Complex[,] {
				{ M11, M12 },
				{ M21, M22 },
			});

			return Ms;

		}

		public Matrix<Complex> interfaceMatrix_p(Layer layer1, Layer layer2)
		{
			// Material parameters
			Complex eps1 = layer1.getMaterial().getEpsilon();
			Complex mu1 = layer1.getMaterial().getMu();
			Complex k1 = Complex.Sqrt(eps1 * mu1) * k0;

			Complex k1z = Complex.Sqrt(k1.Norm() - kx * kx); // norm(z) = ||z||^2

			Complex eps2 = layer2.getMaterial().getEpsilon();
			Complex mu2 = layer2.getMaterial().getMu();
			Complex k2 = Complex.Sqrt(eps2 * mu2) * k0;
			Complex k2z = Complex.Sqrt(k2.Norm() - kx * kx); // norm(z) = ||z||^2

			Complex normImpedance = k2z * eps1 / (k1z * eps2);

			Complex M11 = 0.5 + 0.5 * normImpedance;
			Complex M12 = 0.5 - 0.5 * normImpedance;
			Complex M21 = 0.5 - 0.5 * normImpedance;
			Complex M22 = 0.5 + 0.5 * normImpedance;

			Matrix<Complex> Mp = DenseMatrix.OfArray(new Complex[,] {
				{ M11, M12 },
				{ M21, M22 },
			});

			return Mp;
		}


		public Matrix<Complex> propagate(Layer layer, double distance)
		{
			//complex<double> kz = layer.getKz();

			Complex eps1 = layer.getMaterial().getEpsilon();
			Complex mu1 = layer.getMaterial().getMu();
			Complex k1 = Complex.Sqrt(eps1 * mu1) * k0;

			Complex kz = Complex.Sqrt(k1.Norm() - kx * kx); // norm(z) = ||z||^2
			layer.setKz(kz);

			Complex imagI = new Complex(0.0, 1.0);

			Matrix<Complex> P = DenseMatrix.OfArray(new Complex[,] {
				{ Complex.Exp(-imagI * kz * distance),0},
				{0, Complex.Exp(imagI * kz * distance)},
			});

			return P;

		}





	}
}
