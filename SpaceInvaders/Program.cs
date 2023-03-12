using ConsoleApp17;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

Game game = new(SceneLoader.LoadScene("Scenes/game.scene"));
game.Run();