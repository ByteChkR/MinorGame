using System;
using MinorEngine.exceptions;

namespace MinorGame.exceptions
{
    public class GameException :EngineException
    {
        public GameException(string errorMessage, Exception inner) : base(errorMessage, inner)
        {
        }

        public GameException(string errorMessage) : base(errorMessage)
        {
        }
    }
}