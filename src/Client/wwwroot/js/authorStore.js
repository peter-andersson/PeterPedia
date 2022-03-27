(function () {
  // Based on code from https://github.com/SteveSandersonMS/CarChecker

  const authorStore = "authors";
  const deleteStore = "delete";

  const db = idb.openDB("Authors", 2, {
    upgrade(db) {
      db.deleteObjectStore(authorStore);

      db.createObjectStore(authorStore, { keyPath: "id" }).createIndex("lastUpdated", "lastUpdated");
      db.createObjectStore(deleteStore, { keyPath: "deleted" });
    },
  });

  window.authorStore = {
    get: async (key) => (await db).get(authorStore, key),
    getAll: async () => (await db).getAll(authorStore),
    getDeleted: async () => (await db).getAll(deleteStore),
    getFirstFromIndex: async (indexName, direction) => {
      const cursor = await (await db).transaction(authorStore).store.index(indexName).openCursor(null, direction);
      return (cursor && cursor.value) || null;
    },
    put: async (value) => (await db).put(authorStore, value),
    putDeleted: async (value) => {
      (await db).clear(deleteStore);
      (await db).put(deleteStore, value);
    },
    putAllFromJson: async (json) => {
      const store = (await db).transaction(authorStore, "readwrite").store;
      JSON.parse(json).forEach(item => store.put(item));
    },
    delete: async (key) => (await db).delete(authorStore, key)
  };
})();
