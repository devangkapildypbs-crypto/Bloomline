// Unit Tests for Bloomline — Direction, TileModel, LightSolver, LevelLoader, Stars
using NUnit.Framework;
using Bloomline.Gameplay;
using Bloomline.Levels;

namespace Bloomline.Tests
{
    [TestFixture]
    public class DirectionTests
    {
        [Test]
        public void GetOpposite_ReturnsCorrectDirection()
        {
            Assert.AreEqual(Direction.South, DirectionHelper.GetOpposite(Direction.North));
            Assert.AreEqual(Direction.North, DirectionHelper.GetOpposite(Direction.South));
            Assert.AreEqual(Direction.West, DirectionHelper.GetOpposite(Direction.East));
            Assert.AreEqual(Direction.East, DirectionHelper.GetOpposite(Direction.West));
        }

        [Test]
        public void RotateCW_RotatesCorrectly()
        {
            Assert.AreEqual(Direction.East, DirectionHelper.RotateCW(Direction.North));
            Assert.AreEqual(Direction.South, DirectionHelper.RotateCW(Direction.East));
            Assert.AreEqual(Direction.West, DirectionHelper.RotateCW(Direction.South));
            Assert.AreEqual(Direction.North, DirectionHelper.RotateCW(Direction.West));
        }

        [Test]
        public void RotateOpeningsCW_RotatesMultipleDirections()
        {
            // North+South -> East+West
            Direction vertical = Direction.North | Direction.South;
            Direction rotated = DirectionHelper.RotateOpeningsCW(vertical);
            Assert.AreEqual(Direction.East | Direction.West, rotated);
        }

        [Test]
        public void DegreesToSteps_ConvertsCorrectly()
        {
            Assert.AreEqual(0, DirectionHelper.DegreesToSteps(0));
            Assert.AreEqual(1, DirectionHelper.DegreesToSteps(90));
            Assert.AreEqual(2, DirectionHelper.DegreesToSteps(180));
            Assert.AreEqual(3, DirectionHelper.DegreesToSteps(270));
            Assert.AreEqual(0, DirectionHelper.DegreesToSteps(360));
        }
    }

    [TestFixture]
    public class TileModelTests
    {
        [Test]
        public void StraightTile_AtRotation0_HasNorthSouth()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.Straight, 0, TileColor.White, false);
            Assert.IsTrue(tile.HasOpening(Direction.North));
            Assert.IsTrue(tile.HasOpening(Direction.South));
            Assert.IsFalse(tile.HasOpening(Direction.East));
            Assert.IsFalse(tile.HasOpening(Direction.West));
        }

        [Test]
        public void StraightTile_AtRotation90_HasEastWest()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.Straight, 90, TileColor.White, false);
            Assert.IsTrue(tile.HasOpening(Direction.East));
            Assert.IsTrue(tile.HasOpening(Direction.West));
            Assert.IsFalse(tile.HasOpening(Direction.North));
            Assert.IsFalse(tile.HasOpening(Direction.South));
        }

        [Test]
        public void CornerTile_AtRotation0_HasNorthEast()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.Corner, 0, TileColor.White, false);
            Assert.IsTrue(tile.HasOpening(Direction.North));
            Assert.IsTrue(tile.HasOpening(Direction.East));
            Assert.IsFalse(tile.HasOpening(Direction.South));
            Assert.IsFalse(tile.HasOpening(Direction.West));
        }

        [Test]
        public void Rotate_ChangesTileOpenings()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.Straight, 0, TileColor.White, false);
            Assert.IsTrue(tile.Rotate());
            Assert.AreEqual(1, tile.CurrentRotation);
            Assert.IsTrue(tile.HasOpening(Direction.East));
            Assert.IsTrue(tile.HasOpening(Direction.West));
        }

        [Test]
        public void LockedTile_CannotRotate()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.LockedStraight, 0, TileColor.White, true);
            Assert.IsFalse(tile.CanRotate);
            Assert.IsFalse(tile.Rotate());
        }

        [Test]
        public void Blocker_HasNoOpenings()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.Blocker, 0, TileColor.White, true);
            Assert.IsFalse(tile.HasOpening(Direction.North));
            Assert.IsFalse(tile.HasOpening(Direction.East));
            Assert.IsFalse(tile.HasOpening(Direction.South));
            Assert.IsFalse(tile.HasOpening(Direction.West));
        }

        [Test]
        public void ResetRotation_RestoresToInitial()
        {
            var tile = new TileModel(new GridPosition(0, 0), TileType.Straight, 90, TileColor.White, false);
            tile.Rotate(); // Now at rotation 2 (180°)
            tile.ResetRotation();
            Assert.AreEqual(1, tile.CurrentRotation); // Back to initial 90° = step 1
        }
    }

    [TestFixture]
    public class LightSolverTests
    {
        private TileModel[,] CreateGrid(int width, int height)
        {
            var grid = new TileModel[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    grid[x, y] = new TileModel(new GridPosition(x, y), TileType.Empty, 0, TileColor.White, true);
            return grid;
        }

        [Test]
        public void BasicStraightPath_SourceToFlower_IsComplete()
        {
            // Source -> Straight -> Flower (horizontal)
            var grid = CreateGrid(3, 1);
            grid[0, 0] = new TileModel(new GridPosition(0, 0), TileType.Source, 0, TileColor.White, true);    // East
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Straight, 90, TileColor.White, false); // E-W
            grid[2, 0] = new TileModel(new GridPosition(2, 0), TileType.Flower, 0, TileColor.White, true);     // West

            var result = LightSolver.Solve(grid, 3, 1);
            Assert.IsTrue(result.IsComplete);
            Assert.AreEqual(1, result.PoweredFlowerPositions.Count);
        }

        [Test]
        public void DisconnectedPath_IsNotComplete()
        {
            var grid = CreateGrid(3, 1);
            grid[0, 0] = new TileModel(new GridPosition(0, 0), TileType.Source, 0, TileColor.White, true);    // East
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Straight, 0, TileColor.White, false); // N-S (wrong!)
            grid[2, 0] = new TileModel(new GridPosition(2, 0), TileType.Flower, 0, TileColor.White, true);     // West

            var result = LightSolver.Solve(grid, 3, 1);
            Assert.IsFalse(result.IsComplete);
            Assert.AreEqual(0, result.PoweredFlowerPositions.Count);
            Assert.AreEqual(1, result.UnpoweredFlowerPositions.Count);
        }

        [Test]
        public void CornerPath_ConnectsCorrectly()
        {
            // Source(E) -> Corner(E,S) -> Flower(N)
            var grid = CreateGrid(2, 2);
            grid[0, 1] = new TileModel(new GridPosition(0, 1), TileType.Source, 0, TileColor.White, true);   // East
            grid[1, 1] = new TileModel(new GridPosition(1, 1), TileType.Corner, 90, TileColor.White, false);  // E+S
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Flower, 90, TileColor.White, true);   // North

            var result = LightSolver.Solve(grid, 2, 2);
            Assert.IsTrue(result.IsComplete);
        }

        [Test]
        public void ColoredFlower_OnlyAcceptsMatchingLight()
        {
            var grid = CreateGrid(3, 1);
            grid[0, 0] = new TileModel(new GridPosition(0, 0), TileType.Source, 0, TileColor.Red, true);
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Straight, 90, TileColor.White, false);
            grid[2, 0] = new TileModel(new GridPosition(2, 0), TileType.Flower, 0, TileColor.Blue, true);

            var result = LightSolver.Solve(grid, 3, 1);
            Assert.IsFalse(result.IsComplete); // Red light can't power Blue flower
        }

        [Test]
        public void ColoredFlower_AcceptsMatchingColoredLight()
        {
            var grid = CreateGrid(3, 1);
            grid[0, 0] = new TileModel(new GridPosition(0, 0), TileType.Source, 0, TileColor.Red, true);
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Straight, 90, TileColor.White, false);
            grid[2, 0] = new TileModel(new GridPosition(2, 0), TileType.Flower, 0, TileColor.Red, true);

            var result = LightSolver.Solve(grid, 3, 1);
            Assert.IsTrue(result.IsComplete);
        }

        [Test]
        public void MultipleFlowers_AllMustBePowered()
        {
            // Two flowers, only one connected
            var grid = CreateGrid(3, 2);
            grid[0, 0] = new TileModel(new GridPosition(0, 0), TileType.Source, 0, TileColor.White, true);
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Straight, 90, TileColor.White, false);
            grid[2, 0] = new TileModel(new GridPosition(2, 0), TileType.Flower, 0, TileColor.White, true);
            grid[2, 1] = new TileModel(new GridPosition(2, 1), TileType.Flower, 90, TileColor.White, true); // Disconnected

            var result = LightSolver.Solve(grid, 3, 2);
            Assert.IsFalse(result.IsComplete);
            Assert.AreEqual(1, result.PoweredFlowerPositions.Count);
            Assert.AreEqual(1, result.UnpoweredFlowerPositions.Count);
        }

        [Test]
        public void BlockedPath_LightDoesNotPass()
        {
            var grid = CreateGrid(4, 1);
            grid[0, 0] = new TileModel(new GridPosition(0, 0), TileType.Source, 0, TileColor.White, true);
            grid[1, 0] = new TileModel(new GridPosition(1, 0), TileType.Blocker, 0, TileColor.White, true);
            grid[2, 0] = new TileModel(new GridPosition(2, 0), TileType.Straight, 90, TileColor.White, false);
            grid[3, 0] = new TileModel(new GridPosition(3, 0), TileType.Flower, 0, TileColor.White, true);

            var result = LightSolver.Solve(grid, 4, 1);
            Assert.IsFalse(result.IsComplete);
        }
    }

    [TestFixture]
    public class StarCalculationTests
    {
        [Test]
        public void ThreeStars_WhenUnderPerfectTarget()
        {
            var level = new LevelData
            {
                moveTargetThreeStars = 4,
                moveTargetTwoStars = 6
            };
            Assert.AreEqual(3, level.CalculateStars(3));
            Assert.AreEqual(3, level.CalculateStars(4));
        }

        [Test]
        public void TwoStars_WhenUnderSoftTarget()
        {
            var level = new LevelData
            {
                moveTargetThreeStars = 4,
                moveTargetTwoStars = 6
            };
            Assert.AreEqual(2, level.CalculateStars(5));
            Assert.AreEqual(2, level.CalculateStars(6));
        }

        [Test]
        public void OneStar_WhenOverSoftTarget()
        {
            var level = new LevelData
            {
                moveTargetThreeStars = 4,
                moveTargetTwoStars = 6
            };
            Assert.AreEqual(1, level.CalculateStars(7));
            Assert.AreEqual(1, level.CalculateStars(20));
        }
    }

    [TestFixture]
    public class LevelLoaderTests
    {
        [Test]
        public void ParseTileType_ReturnsCorrectEnum()
        {
            Assert.AreEqual(TileType.Source, LevelLoader.ParseTileType("Source"));
            Assert.AreEqual(TileType.Flower, LevelLoader.ParseTileType("Flower"));
            Assert.AreEqual(TileType.Straight, LevelLoader.ParseTileType("Straight"));
            Assert.AreEqual(TileType.Corner, LevelLoader.ParseTileType("Corner"));
            Assert.AreEqual(TileType.Blocker, LevelLoader.ParseTileType("Blocker"));
            Assert.AreEqual(TileType.LockedStraight, LevelLoader.ParseTileType("LockedStraight"));
            Assert.AreEqual(TileType.LockedCorner, LevelLoader.ParseTileType("LockedCorner"));
            Assert.AreEqual(TileType.Empty, LevelLoader.ParseTileType("Unknown"));
        }

        [Test]
        public void ParseTileColor_ReturnsCorrectEnum()
        {
            Assert.AreEqual(TileColor.White, LevelLoader.ParseTileColor("White"));
            Assert.AreEqual(TileColor.Red, LevelLoader.ParseTileColor("Red"));
            Assert.AreEqual(TileColor.Blue, LevelLoader.ParseTileColor("Blue"));
            Assert.AreEqual(TileColor.Yellow, LevelLoader.ParseTileColor("Yellow"));
            Assert.AreEqual(TileColor.White, LevelLoader.ParseTileColor(null));
        }

        [Test]
        public void LoadFromJson_ParsesLevelData()
        {
            string json = @"{
                ""levelId"": ""test_level"",
                ""chapter"": 1,
                ""levelNumber"": 1,
                ""gridWidth"": 3,
                ""gridHeight"": 3,
                ""moveTargetTwoStars"": 4,
                ""moveTargetThreeStars"": 2,
                ""tutorialText"": ""Test"",
                ""tiles"": [
                    { ""x"": 0, ""y"": 0, ""type"": ""Source"", ""rotation"": 0, ""color"": ""White"", ""locked"": true }
                ]
            }";

            var level = LevelLoader.LoadFromJson(json);
            Assert.IsNotNull(level);
            Assert.AreEqual("test_level", level.levelId);
            Assert.AreEqual(3, level.gridWidth);
            Assert.AreEqual(1, level.tiles.Count);
            Assert.AreEqual("Source", level.tiles[0].type);
        }
    }
}
