using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace SharpTransferMatrix
{
	public class Material
	{
		public Complex Epsilon { get; set; }
		public Complex Mu { get; set; }
		public Complex N { get; set; }
		public string Name { get; set; }

		public Material()
		{
			this.Epsilon = 1.0;
			this.Mu = 1.0;
			N = Complex.Sqrt(Epsilon * Mu);
		} // vacuum constructor
		public Material(Complex epsilon)
		{
			this.Epsilon = epsilon;
			this.Mu = 1.0;
			N = Complex.Sqrt(Epsilon * Mu);
		}
		public Material(Complex epsilon, Complex mu, string name = "")
		{
			this.Epsilon = epsilon;
			this.Mu = mu;
			this.Name = name;
			N = Complex.Sqrt(epsilon * mu);
		}

		public Complex getEpsilon()
		{
			return Epsilon;
		}
		public Complex getMu()
		{
			return Mu;
		}
		public Complex getN()
		{
			return Complex.Sqrt(Epsilon * Mu);
		}


	}
}
