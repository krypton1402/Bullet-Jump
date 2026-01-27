using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Graphics.Animations
{
    public class AnimationStateManager
    {
        private Dictionary<string, Action> _states = new Dictionary<string, Action>();
        private string _currentState;

        public void AddState(string stateName, Action applyAction)
        {
            _states[stateName] = applyAction;
        }

        public void ChangeState(string newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;

            if (_states.TryGetValue(newState, out var applyAction))
            {
                applyAction?.Invoke();
            }
        }

        public string CurrentState => _currentState;
    }
}
