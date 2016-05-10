using System.Collections.Generic;

namespace AdvancedControlsMod.Controls
{
    public interface Control
    {
        float Min { get; set; }
        float Max { get; set; }
        float Center { get; set; }
        bool OnlyPositive { get; set; }
        Axis Axis { get; set; }
        void Apply(float value);
        void Draw();
    }

    public interface ControlGroup
    {
        Dictionary<string, Control> Controls { get; set; }
        string Enabled { get; set; }
        void Apply(float value);
        void Draw();
    }
}

