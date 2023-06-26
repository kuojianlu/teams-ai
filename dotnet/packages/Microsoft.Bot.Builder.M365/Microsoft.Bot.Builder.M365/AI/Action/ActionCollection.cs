﻿
namespace Microsoft.Bot.Builder.M365.AI.Action
{
    public class ActionCollection<TState> : IActionCollection<TState> where TState : TurnState
    {
        private readonly Dictionary<string, ActionEntry<TState>> _actions;

        public ActionCollection()
        {
            _actions = new Dictionary<string, ActionEntry<TState>>();
        }

        /// <inheritdoc />
        public void SetAction(string name, ActionHandler<TState> handler, bool allowOverrides = false)
        {
            if (_actions.ContainsKey(name))
            {
                if (!_actions[name].AllowOverrides)
                {
                    throw new ArgumentException($"Action {name} already exists and does not allow overrides");
                }
            }
            _actions[name] = new ActionEntry<TState>(name, handler, allowOverrides);
        }

        /// <inheritdoc />
        public ActionEntry<TState> GetAction(string name)
        {
            if (!_actions.ContainsKey(name))
            {
                throw new ArgumentException($"`{name}` action does not exist");
            }
            return _actions[name];
        }

        /// <inheritdoc />
        public bool HasAction(string actionName)
        {
            return _actions.ContainsKey(actionName);
        }

    }
}