using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SharpTransferMatrix
{
	public class Layer
	{
		private Complex kz = new Complex();
		private double thickness;
		private Material material = new Material();
		private string name = "";

		public Layer(double thickness, Material material, string name = "")
		{
			this.thickness = thickness;
			this.material = material;
			this.name = name;
		}
		public double getThickness()
		{
			return thickness;
		}
		public Material getMaterial()
		{
			return material;
		}
		public Complex getKz()
		{
			return kz;
		}
		public string getName()
		{
			return name;
		}
		public void setKz(Complex kz_)
		{
			kz = kz_;
		}




	}
}
