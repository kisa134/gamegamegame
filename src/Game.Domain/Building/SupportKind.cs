namespace Game.Domain.Building;

/// <summary>How a building piece is supported. None means "floating in the air" — forbidden.</summary>
public enum SupportKind
{
    None,
    Ground,
    ExistingPiece,
}
