using MauiIcons.Material;

namespace Findy.Models;

public record TrackTarget(
    string Address,
    string? BleName,
    string Title,
    string? Subtitle,
    MaterialIcons KindIcon
);