using AutomizeMyDroughts.Common;
using Bindito.Core;
using UnityEngine;

namespace AutomizeMyDroughts.Components
{
    public interface IAutomatable
    {
        public void Automate(AutomizationEvent.Type e);
    }

    public abstract class Automated : MonoBehaviour, IAutomatable
    {
        protected AutomizationManager Manager => AutomizationManager.Instance;

        public void Start() {
            AutomizationManager.Instance.Register(this);
        }

        public void OnDestroy() {
            AutomizationManager.Instance.Unregister(this);
        }

        abstract public void Automate(AutomizationEvent.Type e);
    }
}
