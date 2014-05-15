using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectSkittles
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Skittle
	{
		#region Properties

		/// <summary>
		/// The color to draw this dude
		/// </summary>
		public Color AverageColor { get; set; }

		/// <summary>
		/// The location to draw this dude
		/// </summary>
		public Rectangle Location { get; set; }

		/// <summary>
		/// The amount to scale the rect when rendering it
		/// </summary>
		public float Scale { get; set; }

		#endregion //Properties

		#region Methods

		public Skittle(Rectangle loc)
		{
			Location = loc;
			Scale = 1.0f;
			AverageColor = Color.White;
		}

		/// <summary>
		/// Get a rect to be rendered
		/// </summary>
		/// <returns></returns>
		public Rectangle RenderRect()
		{
			//Create a matrix to move to the origin

			//Create scale matrix

			//move back to original position

			//set the position of the rect

			//also scale the width & height

			return Location;
		}

		#endregion //Methods
	}
}
