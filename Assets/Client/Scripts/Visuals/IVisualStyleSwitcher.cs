using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Visuals
{
    public enum Styles
    {
        Regular,
        Simple
    }

    public interface IVisualStyleSwitcher
    {
        void ChangeStyle(Styles style);
    }
}