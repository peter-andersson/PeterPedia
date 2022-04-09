(function () {
  // Based on code from https://github.com/SteveSandersonMS/CarChecker

  const episodeStore = "episodes";
  const deleteStore = "delete";

  const db = idb.openDB("Episodes", 1, {
    upgrade(db) {
      const store = db.createObjectStore(episodeStore, { keyPath: "id" });
      store.createIndex("lastUpdate", "lastUpdate");
      db.createObjectStore(deleteStore, { keyPath: "deleted" });
    },
  });

  window.episodeStore = {
    get: async (key) => (await db).get(episodeStore, key),
    getAll: async () => (await db).getAll(episodeStore),
    getDeleted: async () => (await db).getAll(deleteStore),
    getFirstFromIndex: async (indexName, direction) => {
      const cursor = await (await db).transaction(episodeStore).store.index(indexName).openCursor(null, direction);
      return (cursor && cursor.value) || null;
    },
    put: async (value) => (await db).put(episodeStore, value),
    putDeleted: async (value) => {
      (await db).clear(deleteStore);
      (await db).put(deleteStore, value);
    },
    putAllFromJson: async (json) => {
      const store = (await db).transaction(episodeStore, "readwrite").store;
      JSON.parse(json).forEach(item => store.put(item));
    },
    delete: async (key) => (await db).delete(episodeStore, key)
  };
})();
