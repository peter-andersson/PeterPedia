(function () {
  // Based on code from https://github.com/SteveSandersonMS/CarChecker

  const bookStore = "books";
  const deleteStore = "delete";

  const db = idb.openDB("Books", 2, {
    upgrade(db) {
      db.deleteObjectStore(bookStore);

      db.createObjectStore(bookStore, { keyPath: "id" }).createIndex("lastUpdated", "lastUpdated");
      db.createObjectStore(deleteStore, { keyPath: "deleted" });
    },
  });

  window.bookStore = {
    get: async (key) => (await db).get(bookStore, key),
    getAll: async () => (await db).getAll(bookStore),
    getDeleted: async () => (await db).getAll(deleteStore),
    getFirstFromIndex: async (indexName, direction) => {
      const cursor = await (await db).transaction(bookStore).store.index(indexName).openCursor(null, direction);
      return (cursor && cursor.value) || null;
    },
    put: async (value) => (await db).put(bookStore, value),
    putDeleted: async (value) => {
      (await db).clear(deleteStore);
      (await db).put(deleteStore, value);
    },
    putAllFromJson: async (json) => {
      const store = (await db).transaction(bookStore, "readwrite").store;
      JSON.parse(json).forEach(item => store.put(item));
    },
    delete: async (key) => (await db).delete(bookStore, key)
  };
})();
