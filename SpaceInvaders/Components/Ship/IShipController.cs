using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Ship;
internal interface IShipController
{
    Vector2 GetMovementDirection();
}
