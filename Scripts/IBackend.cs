using BabbleCalibration.Scripts.Elements;
using Godot;

namespace BabbleCalibration.Scripts;

public interface IBackend
{
    /// <summary>
    /// Returns the node of this backend
    /// </summary>
    public abstract Node Self { get; }
    /// <summary>
    /// Returns whether or not the backend is an overlay, or a standalone app.<br/>
    /// If a backend is an overlay, a transparent background should be used
    /// </summary>
    public abstract bool IsOverlay { get; }
    /// <summary>
    /// Creates an instance of the backend
    /// </summary>
    /// <returns>The created backend</returns>
    public static abstract IBackend Create();
    /// <summary>
    /// Initializes the backend
    /// </summary>
    public abstract void Initialize();
    /// <summary>
    /// Create an element, attached to the head. Doesn't require parenting.
    /// </summary>
    /// <returns>The created element</returns>
    public abstract ElementBase CreateHeadElement(bool persistent = false);
    /// <summary>
    /// Create an element in free space. Doesn't require parenting.
    /// </summary>
    /// <returns>The created element</returns>
    public abstract ElementBase CreateWorldElement(bool persistent = false);
    /// <summary>
    /// Clears all elements
    /// </summary>
    public abstract void ClearElements();
    /// <summary>
    /// Get global transform of head
    /// </summary>
    /// <returns></returns>
    public abstract Transform3D HeadTransform();
    /// <summary>
    /// Get global transform of an eye
    /// </summary>
    /// <param name="left">If true, left eye, otherwise right</param>
    /// <returns></returns>
    public abstract Transform3D EyeTransform(bool left);
}