﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_Jumper
{
    public partial class MyForm : Form
    {
        Rectangle screen;
        readonly int SizeValue;
        readonly Size BlockSize;
        string currentPath;
        Player player;
        Map map;
        Image playerSkin, border, block, spike, saw, jumpOrb, gravityOrb, finish;

        public MyForm()
        {
            InitializeComponent();
            screen = Screen.FromControl(this).WorkingArea;

            GameTime.Interval = 10;
            GameTime.Tick += new EventHandler(Update);

            KeyDown += new KeyEventHandler(OnPress);
            KeyUp += new KeyEventHandler(OnKeyUp);

            SizeValue = Screen.FromControl(this).WorkingArea.Height / 20 + 1;
            BlockSize = new Size(new Point(SizeValue, SizeValue));

            Initialise();
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    player.IsLeftMoving = false;
                    break;
                case Keys.D:
                    player.IsRightMoving = false;
                    break;
            }
        }

        public void OnPress(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    player.IsLeftMoving = true;
                    break;
                case Keys.D:
                    player.IsRightMoving = true;
                    break;
                case Keys.W:
                    player.IsJumping = true;
                    break;
                case Keys.Space:
                    player.IsJumping = true;
                    break;
                case Keys.Up:
                    player.TriggerTicks = 0;
                    break;
            }
        }

        public void OnMouseClick(object sender, MouseEventArgs e)
        {
            player.TriggerTicks = 0;
        }

        void Initialise()
        {
            currentPath = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName.ToString();
            GetSprite(ref playerSkin, "Cube");
            GetSprite(ref border, "Border");
            GetSprite(ref block, "Block");
            GetSprite(ref spike, "Spike");
            GetSprite(ref saw, "Saw");
            GetSprite(ref jumpOrb, "Yellow_Orb");
            GetSprite(ref gravityOrb, "Blue_Orb");
            GetSprite(ref finish, "Finish");
            map = new Map();
            player = new Player(map.Start, SizeValue);
            GameTime.Start();
            LevelTime.Start();
        }

        private void GetSprite(ref Image inGameSprite, string spriteName)
        {
            inGameSprite = FitInSize(new Bitmap(Path.Combine(currentPath, $"Resources\\{spriteName}.png")));
        }

        public void Update(object sender, EventArgs e)
        {
            player.TriggerTicks++;
            if (player.IsLevelCompleted && !player.IsMessageShowed)
            {
                player.IsMessageShowed = true;
                player.Stop();
                LevelTime.Stop();
                ShowMessage();
            }
            if (player.IsDead)
                Restart();
            if (player.TriggerTicks != 0)
                player.ReactToOrbs(map, SizeValue);
            if (player.IsMoving)
                player.Move(map, SizeValue);

            //Text = $"Position: X = {player.X}, Y = {player.Y}, " +
            //    $"Map pos: ({Math.Round((double)player.X / SizeValue)}; {Math.Round((double)player.Y / SizeValue)})" +
            //    $" Down: ({Math.Floor((double)player.X / SizeValue)}, {Math.Ceiling((double)player.X / SizeValue)}; {Math.Floor((double)player.Y / SizeValue) - 1})" +
            //    $" Up: ({Math.Floor((double)player.X / SizeValue)}, {Math.Ceiling((double)player.X / SizeValue)}; {Math.Ceiling((double)player.Y / SizeValue) + 1})" +
            //    $" Left: ({Math.Ceiling((double)player.X / SizeValue) - 1}; {Math.Floor((double)player.Y / SizeValue)}, {Math.Ceiling((double)player.Y / SizeValue)})" +
            //    $" Right: ({Math.Floor((double)player.X / SizeValue) + 1}; {Math.Floor((double)player.Y / SizeValue)}, {Math.Ceiling((double)player.Y / SizeValue)})";
            
            var time = map.LevelTimeSeconds;
            TimeLabel.Text = $"{time / 60}:{string.Format("{0:00}", time % 60)}".ToString();
            TimeLabel.Location = new Point(screen.Width - TimeLabel.Size.Width, 0);
            Invalidate();
        }

        private void LevelTime_Tick(object sender, EventArgs e)
        {
            map.IncreaseTime();
        }

        private void ShowMessage()
        {
            var time = map.LevelTimeSeconds;
            MessageBox.Show(@$"Вы победили!
Ваше время: {time / 60}:{string.Format("{0:00}", time % 60)}");
        }

        private void Restart()
        {
            player = new Player(map.Start, SizeValue);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawMap(g);
        }

        private void DrawMap(Graphics g)
        {
            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    switch (map.Level[i, j].Type)
                    {
                        case "Border":
                            DrawElement(border, g, i, j);
                            break;
                        case "Block":
                            DrawElement(block, g, i, j);
                            break;
                        case "Spike":
                            DrawElement(spike, g, i, j);
                            break;
                        case "Saw":
                            saw.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            DrawElement(saw, g, i, j);
                            break;
                        case "JumpOrb":
                            DrawElement(jumpOrb, g, i, j);
                            break;
                        case "GravityOrb":
                            DrawElement(gravityOrb, g, i, j);
                            break;
                        case "Finish":
                            DrawElement(finish, g, i, j);
                            break;
                    }
            g.DrawImage(playerSkin, ConvertMathToWorld(player.X, player.Y));
        }

        private void DrawElement(Image e, Graphics g, int i, int j)
        {
            g.DrawImage(e, ConvertMathToWorld(i * SizeValue, j * SizeValue));
        }

        private Image FitInSize(Image image) =>
            new Bitmap(image, BlockSize);

        private Point ConvertMathToWorld(int x, int y) =>
            new Point(x, (map.Height - 1) * SizeValue - y);
    }
}
