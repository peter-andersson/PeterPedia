(function () {
  // Based on code from https://github.com/SteveSandersonMS/CarChecker

  const storename = "books";

  const db = idb.openDB("Books", 1, {
    upgrade(db) {
      db.createObjectStore(storename, { keyPath: "id" }).createIndex("lastUpdated", "lastUpdated");
    },
  });

  window.bookStore = {
    get: async (key) => (await db).transaction(storename).store.get(key),
    getAll: async () => (await db).transaction(storename).store.getAll(),
    getFirstFromIndex: async (indexName, direction) => {
      const cursor = await (await db).transaction(storename).store.index(indexName).openCursor(null, direction);
      return (cursor && cursor.value) || null;
    },
    put: async (value) => (await db).transaction(storename, "readwrite").store.put(value),
    putAllFromJson: async (json) => {
      const store = (await db).transaction(storename, "readwrite").store;
      JSON.parse(json).forEach(item => store.put(item));
    },
    delete: async (key) => (await db).transaction(storename, "readwrite").store.delete(key)
  };
})();
