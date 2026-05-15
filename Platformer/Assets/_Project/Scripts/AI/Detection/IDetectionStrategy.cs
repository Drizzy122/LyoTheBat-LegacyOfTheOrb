using UnityEngine;
using ImprovedTimers;

namespace Platformer {
    public interface IDetectionStrategy {
        bool Execute(Transform player, Transform detector, CountdownTimer timer);
    }
}