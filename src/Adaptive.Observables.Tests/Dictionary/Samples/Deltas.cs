using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;

namespace Adaptive.Observables.Tests.Samples
{
    /// <summary>
    /// This demonstrates receiving a stream of updates from a server which are made up of an initial full snapshot, then deltas.
    /// </summary>
    [TestFixture]
    public class Deltas
    {
        private Subject<Either<FullUpdate, DeltaUpdate>> _serverUpdateStream = new Subject<Either<FullUpdate, DeltaUpdate>>();
        private IObservable<DictionaryModification<string, Update>> _dictionaryModificationStream;
        private IObservableDictionary<string, Update> _observableDictionary;

        [SetUp]
        public void SetUp()
        {
            _serverUpdateStream = new Subject<Either<FullUpdate, DeltaUpdate>>();

            _dictionaryModificationStream = _serverUpdateStream
                .Select(either =>
                {
                    if (either.IsLeft)
                    {
                        return DictionaryModification.Replace(either.Left.Values["key"], (Update)either.Left);
                    }
                    else
                    {
                        return DictionaryModification.Upset(either.Right.Values["key"], (Update)either.Right);
                    }
                });

            _observableDictionary = _dictionaryModificationStream.ToObservableDictionary((key, existing, update) =>
            {
                var fullUpdate = new FullUpdate(key)
                {
                    Values = new Dictionary<string, string>(existing.Values)
                };

                foreach (var kvp in update)
                {
                    fullUpdate.Values[kvp.Key] = kvp.Value;
                }

                return fullUpdate;
            });
        }

        [Test]
        public void Initial_snapshot_and_subsequent_deltas_yield_full_snapshot_each_time()
        {
            var key = "EURUSD";
            // arrange
            var observations = new List<Update>();
            _observableDictionary.Get(key)
                .Only(DictionaryNotificationType.Inserted, DictionaryNotificationType.Updated) // ignore meta notifications
                .Select(dn => dn.Value) // select out the new value in the dictionary
                .Subscribe(observations.Add);

            // act
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new FullUpdate(key) { { "bid", "1.234"}, { "ask", "1.334"}, { "valueDate", "2014-07-16"} }));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "bid", "1.233" }}));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "ask", "1.333" }}));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "bid", "1.231" }, { "ask", "1.331" }}));

            // assert

            observations.ForEach(update => Assert.AreEqual(4, update.Values.Count));
            var first = observations.First();
            var last = observations.Last();

            Assert.AreEqual("1.234", first.Values["bid"]);
            Assert.AreEqual("1.334", first.Values["ask"]);

            Assert.AreEqual("1.231", last.Values["bid"]);
            Assert.AreEqual("1.331", last.Values["ask"]);
        }

        [Test]
        public void Initial_snapshot_and_subsequent_deltas_yield_full_snapshot_then_delta_each_time()
        {
            var key = "EURUSD";
            // arrange
            var observations = new List<Update>();
            _observableDictionary.Get(key)
                .Only(DictionaryNotificationType.Inserted, DictionaryNotificationType.Existing, DictionaryNotificationType.Updated) // ignore meta notifications
                .Select(dn =>
                    dn.Type == DictionaryNotificationType.Inserted
                    ? dn.Value
                    : dn.UpdatingValue
                ) // select out the new value in the dictionary
                .Subscribe(observations.Add);

            // act
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new FullUpdate(key) { { "bid", "1.234" }, { "ask", "1.334" }, { "valueDate", "2014-07-16" } }));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "bid", "1.233" } }));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "ask", "1.333" } }));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "bid", "1.231" }, { "ask", "1.331" }}));
            // assert

            Assert.AreEqual(4, observations[0].Values.Count);
            Assert.AreEqual(2, observations[1].Values.Count);
            Assert.AreEqual(2, observations[2].Values.Count);
            Assert.AreEqual(3, observations[3].Values.Count);
        }

        [Test]
        public void Subscription_to_key_always_yields_full_snapshot_first()
        {
            var key = "EURUSD";
            // arrange
            var firstObservations = new List<Update>();
            _observableDictionary.Get(key)
                .Only(DictionaryNotificationType.Inserted, DictionaryNotificationType.Existing, DictionaryNotificationType.Updated) // ignore meta notifications
                .Select(dn =>
                    dn.Type == DictionaryNotificationType.Inserted
                    ? dn.Value
                    : dn.UpdatingValue
                ) // select out the new value in the dictionary
                .Subscribe(firstObservations.Add);

            // act
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new FullUpdate(key) { { "bid", "1.234" }, { "ask", "1.334" }, { "valueDate", "2014-07-16" } }));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "bid", "1.233" } }));
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "ask", "1.333" } }));
            
            var secondObservations = new List<Update>();
            _observableDictionary.Get(key)
                .Only(DictionaryNotificationType.Inserted, DictionaryNotificationType.Existing, DictionaryNotificationType.Updated) // ignore meta notifications
                .Select(dn =>
                    dn.Type == DictionaryNotificationType.Updated
                    ? dn.UpdatingValue
                    : dn.Value
                ) // select out the new value in the dictionary
                .Subscribe(secondObservations.Add);

            
            _serverUpdateStream.OnNext(new Either<FullUpdate, DeltaUpdate>(new DeltaUpdate(key) { { "bid", "1.231" }, { "ask", "1.331" } }));
            // assert

            Assert.AreEqual(4, firstObservations.Count);
            Assert.AreEqual(2, secondObservations.Count);

            Assert.AreEqual(4, secondObservations[0].Values.Count);

        }
    }
}