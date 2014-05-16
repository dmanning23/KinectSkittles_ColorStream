using Microsoft.Kinect;
using System.Diagnostics;
using ResolutionBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace KinectSkittles
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		#region Members

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Texture2D _circle;

		List<Skittle> Skittles { get; set; }

		/// <summary>
		/// Active Kinect sensor
		/// </summary>
		private KinectSensor sensor;

		/// <summary>
		/// Intermediate storage for the depth data converted to color
		/// </summary>
		private byte[] colorPixels;

		Texture2D pixels;
		Color[] pixelData_clear;

		int ScreenX = 1024;
		int ScreenY = 768;
		int CellSize = 16;

		#endregion //Members

		#region Methods
		
		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			Skittles = new List<Skittle>();

			// Change Virtual Resolution
			Resolution.Init(ref graphics);
			Resolution.SetDesiredResolution(ScreenX, ScreenY);
			Resolution.SetScreenResolution(1280, 720, false);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//Create all the skittles
			for (int i = 0; i < ScreenX; i += CellSize)
			{
				for (int j = 0; j < ScreenY; j += CellSize)
				{
					Skittles.Add(new Skittle(new Rectangle(i, j, CellSize, CellSize)));
				}
			}

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			_circle = Content.Load<Texture2D>("circle");

			pixels = new Texture2D(graphics.GraphicsDevice,
				640,
				480, false, SurfaceFormat.Color);
			pixelData_clear = new Color[640 * 480];
			for (int i = 0; i < pixelData_clear.Length; ++i)
				pixelData_clear[i] = Color.Black;

			// Look through all sensors and start the first connected one.
			// This requires that a Kinect is connected at the time of app startup.
			// To make your app robust against plug/unplug, 
			// it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
			foreach (var potentialSensor in KinectSensor.KinectSensors)
			{
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			if (null != this.sensor)
			{
				// Turn on the color stream to receive color frames
				this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

				// Allocate space to put the color pixels we'll create
				this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

				// Add an event handler to be called whenever there is new color frame data
				this.sensor.ColorFrameReady += this.SensorColorFrameReady;

				// Start the sensor!
				try
				{
					this.sensor.Start();
				}
				catch (IOException)
				{
					this.sensor = null;
				}
			}

			//if (null == this.sensor)
			//{
			//	this.statusBarText.Text = Properties.Resources.NoKinectReady;
			//}
		}


		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			if (null != this.sensor)
			{
				this.sensor.Stop();
			}
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin();

			for (int i = 0; i < Skittles.Count; i++)
			{
				spriteBatch.Draw(_circle, Skittles[i].Location, Skittles[i].AverageColor.Average());
			}

			spriteBatch.End();

			pixels.SetData<Color>(pixelData_clear);
			spriteBatch.Begin();
			spriteBatch.Draw(pixels, new Vector2(0, 0), null, Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Event handler for Kinect sensor's ColorFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
		{
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					// Copy the pixel data from the image to a temporary array
					colorFrame.CopyPixelDataTo(this.colorPixels);

					//get the width of the image
					int imageWidth = colorFrame.Width;

					//get the height of the image
					int imageHeight = colorFrame.Height;

					//get the num cells for each axis
					int cellsX = ScreenX / CellSize;
					int cellsY = ScreenY / CellSize;

					 // Convert the depth to RGB
					for (int colorIndex = 0; colorIndex < colorPixels.Length; colorIndex += 4)
					{
						//get the pixel column
						int x = colorIndex % colorPixels.Length;

						//get the pixel row
						int y = colorIndex / colorPixels.Length;

						//convert the image x to cell x
						int x2 = (x * cellsX) / imageWidth;

						//convert the image y to cell y
						int y2 = (y * cellsY) / imageHeight;

						//get the index of the cell
						int cellIndex = (y2 * cellsY) + x2;
						Debug.Assert(cellIndex < Skittles.Count);

						//Create a new color
						Color pixelColor = new Color(colorPixels[colorIndex + 2], colorPixels[colorIndex + 1], colorPixels[colorIndex + 0]);

						//add to the cell color
						Skittles[cellIndex].AverageColor.Add(pixelColor);
					}
				}
			}
		}

		#endregion //Methods
	}
}
