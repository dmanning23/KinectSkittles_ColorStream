using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ResolutionBuddy;
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

		private const int ScreenX = 1024;
		private const int ScreenY = 768;
		private const int CellSize = 16;

		//get the num cells for each axis
		private const int CellsX = ScreenX / CellSize;
		private const int CellsY = ScreenY / CellSize;

		IResolution _resolution;

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			Skittles = new List<Skittle>();

			_resolution = new ResolutionComponent(this, graphics, new Point(1280, 720), new Point(1280, 720), false, false);
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
			for (int j = 0; j < ScreenY; j += CellSize)
			{
				for (int i = 0; i < ScreenX; i += CellSize)
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
				this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

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
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
			Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();

			spriteBatch.Begin(SpriteSortMode.Immediate,
			BlendState.AlphaBlend,
			null, null, null, null,
			Resolution.TransformationMatrix());

			for (int i = 0; i < Skittles.Count; i++)
			{
				spriteBatch.Draw(_circle, Skittles[i].Location, new Color(Skittles[i].AverageColor.Average()));
			}

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

					// // Convert the depth to RGB
					//for (int colorIndex = 0; colorIndex < colorPixels.Length; colorIndex += 4)
					//{
					//	//get the pixel column
					//	int x = (colorIndex / 4) % imageWidth;

					//	//get the pixel row
					//	int y = (colorIndex / 4) / imageWidth;

					//	//convert the image x to cell x
					//	int x2 = (x * CellsX) / imageWidth;

					//	//convert the image y to cell y
					//	int y2 = (y * CellsY) / imageHeight;

					//	//get the index of the cell
					//	int cellIndex = (y2 * CellsX) + x2;
						
					//	//Create a new color
					//	Color pixelColor = new Color(colorPixels[colorIndex + 2], colorPixels[colorIndex + 1], colorPixels[colorIndex + 0]);

					//	//add to the cell color
					//	Skittles[cellIndex].AverageColor.Add(pixelColor.ToVector3());
					//}

					// Convert the depth to RGB
					for (int pixelIndex = 0; pixelIndex < Skittles.Count; pixelIndex++)
					{
						//get the pixel column
						int x = pixelIndex % CellsX;

						//get the pixel row
						int y = pixelIndex / CellsX;

						//convert the image x to cell x
						int x2 = (x * imageWidth) / CellsX;

						//convert the image y to cell y
						int y2 = (y * imageHeight) / CellsY;

						//get the index of the cell
						int imageIndex = ((y2 * imageWidth) + x2) * 4;

						//Create a new color
						Color pixelColor = new Color(colorPixels[imageIndex + 2], colorPixels[imageIndex + 1], colorPixels[imageIndex + 0]);
						Skittles[pixelIndex].AverageColor.Add(pixelColor.ToVector3());
					}
				}
			}
		}

		#endregion //Methods
	}
}
