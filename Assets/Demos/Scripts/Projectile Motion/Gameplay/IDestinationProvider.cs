using UnityEngine;

public interface IDestinationProvider
{
    /// <summary>Return the next position to move to. Returns false if no destination is available.</summary>
    bool TryGetNext(out Vector3 position);
}
