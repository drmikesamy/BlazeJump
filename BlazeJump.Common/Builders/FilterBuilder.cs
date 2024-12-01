﻿using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Builders
{
    public class FilterBuilder
    {
        private List<Filter> _filters = new List<Filter>();
        private Filter _filter;

        public FilterBuilder AddFilter()
        {
            _filter = new Filter();
            _filters.Add(_filter);
            return this;
        }

        public FilterBuilder AddKind(KindEnum kind)
        {
            if (_filter.Kinds == null)
            {
                _filter.Kinds = new List<int>();
            }

            _filter.Kinds.Add((int)kind);
            return this;
        }

        public FilterBuilder WithFromDate(DateTime fromDate)
        {
            _filter.Since = fromDate;
            return this;
        }

        public FilterBuilder WithToDate(DateTime toDate)
        {
            _filter.Until = toDate;
            return this;
        }

        public FilterBuilder WithLimit(int limit)
        {
            _filter.Limit = limit;
            return this;
        }

        public FilterBuilder AddTaggedEventId(string taggedEventId)
        {
            if (_filter.TaggedEventIds == null)
            {
                _filter.TaggedEventIds = new List<string>();
            }

            _filter.TaggedEventIds.Add(taggedEventId);
            return this;
        }

        public FilterBuilder AddTaggedEventIds(List<string> taggedEventIds)
        {
            if (_filter.TaggedEventIds == null)
            {
                _filter.TaggedEventIds = new List<string>();
            }

            _filter.TaggedEventIds.AddRange(taggedEventIds);
            return this;
        }
        public FilterBuilder AddTaggedKeywords(List<string> taggedKeywords)
        {
            if (_filter.TaggedKeywords == null)
            {
                _filter.TaggedKeywords = new List<string>();
            }

            _filter.TaggedKeywords.AddRange(taggedKeywords);
            return this;
        }
        public FilterBuilder AddTaggedKeyword(string taggedKeyword)
        {
            if (_filter.TaggedKeywords == null)
            {
                _filter.TaggedKeywords = new List<string>();
            }

            _filter.TaggedKeywords.Add(taggedKeyword);
            return this;
        }

        public FilterBuilder AddEventId(string eventId)
        {
            if (_filter.EventIds == null)
            {
                _filter.EventIds = new List<string>();
            }

            _filter.EventIds.Add(eventId);
            return this;
        }

        public FilterBuilder AddEventIds(List<string> eventIds)
        {
            if (_filter.EventIds == null)
            {
                _filter.EventIds = new List<string>();
            }

            _filter.EventIds.AddRange(eventIds);
            return this;
        }

        public FilterBuilder AddAuthor(string author)
        {
            if (_filter.Authors == null)
            {
                _filter.Authors = new List<string>();
            }

            _filter.Authors.Add(author);
            return this;
        }

        public FilterBuilder AddAuthors(List<string> authors)
        {
            if (_filter.Authors == null)
            {
                _filter.Authors = new List<string>();
            }

            _filter.Authors.AddRange(authors);
            return this;
        }
        public FilterBuilder AddSearch(string query)
        {
            _filter.Search = query;
            return this;
        }

        public List<Filter> Build()
        {
            _filters.Where(f => f.Since == null).ToList().ForEach(f => f.Since = DateTime.Now.AddYears(-20));
            _filters.Where(f => f.Until == null).ToList().ForEach(f => f.Until = DateTime.Now.AddDays(1));
            foreach (var filter in _filters.ToList())
            {
                if ((filter.Authors == null || filter.Authors.Count == 0)
                    && (filter.Search == null)
                    && (filter.TaggedEventIds == null || filter.TaggedEventIds.Count == 0)
                    && (filter.TaggedKeywords == null || filter.TaggedKeywords.Count == 0)
                    && (filter.Kinds == null || filter.Kinds.Count == 0)
                    && (filter.EventIds == null || filter.EventIds.Count == 0))
                {
                    _filters.Remove(filter);
                }
            }
            return _filters;
        }
    }
}