using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleSnake
{
    public partial class Form1 : Form
    {
        //Enum to easily check directions
        enum Direction { Up=0,Down=1,Left=2,Right=3};
        Direction direction;
        //Numbers to keep time and snake length.
        float fTimePassed;
        int iTimeCounter;
        int iSnakeLength;
        //Booleans to check which state the game is in.
        bool isSnakeGrowing;
        bool isSnakeMoving;
        bool isGamePaused = true;
        //Arrays for the snake and a temporary one to increase snake size.
        Point[] pointSnakePositions;
        Point[] pointTempSnake;
        //Brushes and graphics for the visuals.
        SolidBrush headBrush = new SolidBrush(Color.BlueViolet);
        SolidBrush bodyBrush = new SolidBrush(Color.ForestGreen);
        Graphics gPanel;
        //Create the random seed only once so that it doesn't have to be done again.
        Random rnd = new Random();
        
        

        public Form1()
        {
            //Initializes form.
            InitializeComponent();
            //Does the starting requirements such as assigning start button, assigning keypresses,
            //making sure the timer is off and allowing the panel to be painted on.
            tmrGame.Enabled = false;
            btnGame.Click += btnGame_Click_Start;
            this.KeyDown += new KeyEventHandler(HandleMovement);
            gPanel = pnlGame.CreateGraphics();
        }

        //Pressed whenever the user wants to start the game.
        private void btnGame_Click_Start(object sender, EventArgs e)
        {
            //Refreshes all the variables for the start of the game.
            fTimePassed = 0;
            iTimeCounter = 0;
            iSnakeLength = 0;
            isSnakeGrowing = false;
            isSnakeMoving = true; 

            //Remove the start function from the button, added the pause function to the button and allowed and set the game to allow input.
            btnGame.Click -= btnGame_Click_Start;
            btnGame.Text = "Pause";
            btnGame.Click += btnGame_Click_Pause;
            isGamePaused = false;
            
            //Creates a new snake, in a random position, going in a random direction.
            pointSnakePositions = new Point[1];
            int iXPosition = rnd.Next(0, 20);
            int iYPosition = rnd.Next(0, 20);
            pointSnakePositions[0]=new Point(iXPosition,iYPosition);
            iSnakeLength++;
            int iRandDirection = rnd.Next(0, 4);
            direction = (Direction)iRandDirection;

            //Sets the timer to start which handles all the game input via the intervals.
            tmrGame.Enabled = true;
        }

        //Pressed when the user wants to pause the game.
        private void btnGame_Click_Pause(object sender, EventArgs e)
        {
            //Disallow the game to allow input, remove the pause function from the button,
            //added the resume function to the button and placed the timer on hold.
            tmrGame.Enabled = false;
            isGamePaused = true;
            btnGame.Click -= btnGame_Click_Pause;
            btnGame.Text = "Resume";
            btnGame.Click += btnGame_Click_Resume;
        }

        //Pressed when the user wants to resume the game on pause.
        private void btnGame_Click_Resume(object sender, EventArgs e)
        {
            //Allow the game to allow input, remove the resume function from the button,
            //added the pause function to the button and continued the timer.
            isGamePaused = false;
            btnGame.Click -= btnGame_Click_Resume;
            btnGame.Text = "Pause";
            btnGame.Click += btnGame_Click_Pause;
            tmrGame.Enabled = true;
        }

        //Runs whenever the timer reaches an interval of 0.1 seconds.
        private void tmrGame_Tick(object sender, EventArgs e)
        {
            //
            fTimePassed += (float)tmrGame.Interval/1000;
            lblTime.Text = "Time passed: " + fTimePassed.ToString("0.0") + "s";

            //Every 10 intervals the snake is allowed to grow.
            iTimeCounter++;
            if (iTimeCounter == 10)
            {
                iTimeCounter = 0;
                isSnakeGrowing = true;
                iSnakeLength++;
            }

            //Snake positioning, visuals and growth are handled here.
            SnakeMoveGrow();
            DrawSnake();
            lblLength.Text = "Length of snake: " + iSnakeLength.ToString();

            //Checks whether the snake is in a position to die or not.
            CheckSnakeDeath();
        }
        
        //Called when the snake needs to be drawn.
        private void DrawSnake()
        {
            //Resets the panel.
            gPanel.Clear(Color.White);

            //Goes through the snake's body parts and paints them in the position they are in. They are
            //multiplied by 20 to correspond with the panel.
            for (int i = 1; i < pointSnakePositions.Length; i++)
            {
                
                    gPanel.FillRectangle(bodyBrush, pointSnakePositions[i].X * 20, pointSnakePositions[i].Y * 20, 20, 20);
            }

            //The head is drawn last to correctly display collisions.
            gPanel.FillRectangle(headBrush, pointSnakePositions[0].X * 20, pointSnakePositions[0].Y * 20, 20, 20);
        }

        //Runs when the user presses a key and it determines whether the input is valid.
        private void HandleMovement(object sender, KeyEventArgs e)
        {
            //First of all it is checking to make sure that the snake is in the position to move
            //so that the user can't quickly change inputs and make an impossible move.
            if (isSnakeMoving) 
            { 
                //Then it checks to see that the game is running so that the user can't input movement from pause.
                if (!isGamePaused)
                {
                    //Then, corresponding to the key, the movement direction is changed accordingly if
                    //ths move is a possible move.
                    if (e.KeyCode == Keys.W && direction!=Direction.Down)
                        direction = Direction.Up;
                
                    if (e.KeyCode == Keys.A && direction != Direction.Right)
                        direction = Direction.Left;
                
                    if (e.KeyCode == Keys.S && direction != Direction.Up)
                        direction = Direction.Down;
                
                    if (e.KeyCode == Keys.D && direction != Direction.Left)
                        direction = Direction.Right;
                }
                //Sets it so the snake can't recieve new input until the original has been carried out.
                isSnakeMoving = false;
            }

        }
        
        //Is called whenever the snake needs to move or grow.
        private void SnakeMoveGrow()
        {
            //Points created for assigning and storing the new positions of the snake.
            Point pointPrevious;
            Point pointTemp;

            //A switch statement, depending on the direction snake is going.
            //For all the outcomes the first thing that is done is the head is assigned to pointPrevious.
            //Then it is checked if the snake head is about to go through a border. 
            //If it is move it to the exact opposite side. If it isn't just move it in the corresponding direction
            //one unit. Then if the snake is growing, use the class level temporary array and give size one larger than
            //original. Assign the first point with where the head is, the second with where the head use to be
            //and the rest follow as normal. If the snake is not growing and it is not a size of one then it just needs to move regularly. The second
            //part has its location stored then it's moved to where the the head used to be. The rest follow this trend moving to 
            //where the last body part was.
            switch (direction)
            {
                case Direction.Up:
                    pointPrevious = pointSnakePositions[0];
                    if (pointSnakePositions[0].Y == 0)
                        pointSnakePositions[0].Y = 19;

                    else
                        pointSnakePositions[0].Y--;

                    if (isSnakeGrowing)
                    {
                        pointTempSnake = new Point[pointSnakePositions.Length + 1];
                        pointTempSnake[0] = pointSnakePositions[0];
                        pointTempSnake[1] = pointPrevious;
                        for(int i = 2; i <= pointSnakePositions.Length; i++)
                        {
                            pointTempSnake[i] = pointSnakePositions[i - 1];
                        }
                        pointSnakePositions = pointTempSnake;
                    }

                    else if (pointSnakePositions.Length != 1) { 
                        for(int i = 1; i < pointSnakePositions.Length; i++)
                        {
                            pointTemp = pointPrevious;
                            pointPrevious = pointSnakePositions[i];
                            pointSnakePositions[i] = pointTemp;
                        }
                    }
                    break;

                case Direction.Down:
                    pointPrevious = pointSnakePositions[0];
                    if (pointSnakePositions[0].Y == 19)
                        pointSnakePositions[0].Y = 0;

                    else
                        pointSnakePositions[0].Y++;

                    if (isSnakeGrowing)
                    {
                        pointTempSnake = new Point[pointSnakePositions.Length + 1];
                        pointTempSnake[0] = pointSnakePositions[0];
                        pointTempSnake[1] = pointPrevious;
                        for (int i = 2; i <= pointSnakePositions.Length; i++)
                        {
                            pointTempSnake[i] = pointSnakePositions[i - 1];
                        }
                        pointSnakePositions = pointTempSnake;
                    }

                    else if (pointSnakePositions.Length != 1)
                    {
                        for (int i = 1; i < pointSnakePositions.Length; i++)
                        {
                            pointTemp = pointPrevious;
                            pointPrevious = pointSnakePositions[i];
                            pointSnakePositions[i] = pointTemp;
                        }
                    }
                    break;

                case Direction.Left:
                    pointPrevious = pointSnakePositions[0];
                    if (pointSnakePositions[0].X == 0)
                        pointSnakePositions[0].X = 19;

                    else
                        pointSnakePositions[0].X--;
                    
                    if (isSnakeGrowing)
                    {
                        pointTempSnake = new Point[pointSnakePositions.Length + 1];
                        pointTempSnake[0] = pointSnakePositions[0];
                        pointTempSnake[1] = pointPrevious;
                        for (int i = 2; i <= pointSnakePositions.Length; i++)
                        {
                            pointTempSnake[i] = pointSnakePositions[i - 1];
                        }
                        pointSnakePositions = pointTempSnake;
                    }

                    else if (pointSnakePositions.Length != 1)
                    {
                        for (int i = 1; i < pointSnakePositions.Length; i++)
                        {
                            pointTemp = pointPrevious;
                            pointPrevious = pointSnakePositions[i];
                            pointSnakePositions[i] = pointTemp;
                        }
                    }
                    break;

                case Direction.Right:
                    pointPrevious = pointSnakePositions[0];
                    if (pointSnakePositions[0].X == 19)
                        pointSnakePositions[0].X = 0;

                    else
                        pointSnakePositions[0].X++;

                    if (isSnakeGrowing)
                    {
                        pointTempSnake = new Point[pointSnakePositions.Length + 1];
                        pointTempSnake[0] = pointSnakePositions[0];
                        pointTempSnake[1] = pointPrevious;
                        for (int i = 2; i <= pointSnakePositions.Length; i++)
                        {
                            pointTempSnake[i] = pointSnakePositions[i - 1];
                        }
                        pointSnakePositions = pointTempSnake;
                    }

                    else if (pointSnakePositions.Length != 1)
                    {
                        for (int i = 1; i < pointSnakePositions.Length; i++)
                        {
                            pointTemp = pointPrevious;
                            pointPrevious = pointSnakePositions[i];
                            pointSnakePositions[i] = pointTemp;
                        }
                    }
                    break;
            }
            //Stops the snake from growing.
            isSnakeGrowing = false;

            //Allows the snake to recieve input again.
            isSnakeMoving = true;
        }

        //Is called to check if the snake should be dead or not.
        private void CheckSnakeDeath()
        {
            //Creates a boolean that if, the snake should be dead it will be true.
            bool isSnakeDead = false;

            //A for loop that checks every body part past the third snakepart (because it is impossible for the snake head to touch these)
            //with the head and sees if they share the same cooordinates. The loop stops if the condition is met early.
            for(int i = 3; i < pointSnakePositions.Length&&!isSnakeDead; i++)
            {
                if (pointSnakePositions[0] == pointSnakePositions[i])
                    isSnakeDead = true;
            }

            //Runs if the snake is dead.
            if (isSnakeDead) 
            { 
                //Sets the timer to stop (which halts all game handling), removes the pause function
                //from the button, adds the start function to the button and informs the user that they have died.
                tmrGame.Enabled = false;
                btnGame.Click -= btnGame_Click_Pause;
                btnGame.Click += btnGame_Click_Start;
                btnGame.Text = "Start";
                MessageBox.Show("You Died!");
            }
        }
    }
}
