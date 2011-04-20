using System;
using System.Collections.Generic;
using FubuFastPack.Persistence;
using FubuMusicStore.Domain;
using FubuMVC.Core;
using FubuMVC.Core.View;
using System.Linq;

namespace FubuMusicStore.Actions.Partials
{
    public class GenreListPartialAction
    {
        private readonly IRepository _repository;

        public GenreListPartialAction(IRepository repository)
        {
            _repository = repository;
        }

        [FubuPartial]
        public GenreListModel Get(GenreListRequest request)
        {
            return new GenreListModel() {Genres = _repository.Query<Genre>().OrderBy(x => x.Name).ToList()};
        }
    }

    public class GenreListModel
    {
        public IList<Genre> Genres { get; set; }
    }

    public class GenreListRequest{}

    public class GenreListControl : FubuControl<GenreListModel>{}
}