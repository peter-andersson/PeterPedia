(function () {
  // Based on code from https://github.com/SteveSandersonMS/CarChecker

  const movieStore = "movies";
  const deleteStore = "delete";

  const db = idb.openDB("Movies", 1, {
    upgrade(db) {
      const store = db.createObjectStore(movieStore, { keyPath: "id" });
      store.createIndex("lastUpdate", "lastUpdate");
      db.createObjectStore(deleteStore, { keyPath: "deleted" });
    },
  });

  window.movieStore = {
    get: async (key) => (await db).get(movieStore, key),
    getAll: async () => (await db).getAll(movieStore),
    getDeleted: async () => (await db).getAll(deleteStore),
    getFirstFromIndex: async (indexName, direction) => {
      const cursor = await (await db).transaction(movieStore).store.index(indexName).openCursor(null, direction);
      return (cursor && cursor.value) || null;
    },
    put: async (value) => (await db).put(movieStore, value),
    putDeleted: async (value) => {
      (await db).clear(deleteStore);
      (await db).put(deleteStore, value);
    },
    putAllFromJson: async (json) => {
      const store = (await db).transaction(movieStore, "readwrite").store;
      JSON.parse(json).forEach(item => store.put(item));
    },
    delete: async (key) => (await db).delete(movieStore, key)
  };
})();
