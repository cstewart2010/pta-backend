using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal sealed class IndexCollectionImpl<TIndex>(ICollection<TIndex> collection) : BaseCollectionImpl<TIndex>
    where TIndex : IDocument, IDexDocument
{
    public override ICollection<TIndex> Collection { get; protected set; } = collection;
}