import React, { useCallback, useEffect, useRef, useState } from "react";
import { toast } from "sonner";
import { getApiErrorMessage } from "@/api";

type WithId = { id: string };

export interface DictionaryCrudData<TItem, TRelated> {
  /** Rows rendered by the page table. */
  items: TItem[];
  /** Optional reference list used by selects / pickers on the same page. */
  related?: TRelated[];
}

export interface UseDictionaryCrudOptions<TItem extends WithId, TRelated> {
  /** Loads the list (and optional reference list) shown by the page. */
  fetch: () => Promise<DictionaryCrudData<TItem, TRelated>>;
  /** Text (or texts) a row is matched against by the search box. */
  searchText: (item: TItem) => string | string[];
  /** Value of the shared `name` field when editing an item. */
  formName?: (item: TItem) => string;
  /** Populates page-specific form fields. Receives `null` when creating. */
  fillForm?: (item: TItem | null) => void;
  /** Returns false to abort submitting (page-level validation). */
  validate?: () => boolean;
  /** Performs the create/update request. */
  submit: (editingId: string | null) => Promise<void>;
  remove?: (id: string) => Promise<unknown>;
  restore?: (id: string) => Promise<unknown>;
  /** `window.confirm` text shown before deleting. No prompt when omitted. */
  confirmDelete?: (item: TItem) => string;
  deleteSuccessMessage?: string;
  deleteErrorMessage?: string;
  restoreSuccessMessage?: string;
  restoreErrorMessage?: string;
  saveErrorMessage?: string;
}

/**
 * Shared state and handlers for the admin dictionary CRUD pages: list loading,
 * search filtering, modal open/edit state and the delete/restore/save flows.
 */
export const useDictionaryCrud = <TItem extends WithId, TRelated = unknown>(
  options: UseDictionaryCrudOptions<TItem, TRelated>,
) => {
  const [items, setItems] = useState<TItem[]>([]);
  const [related, setRelated] = useState<TRelated[]>([]);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");

  // Keeps the callbacks below stable while still calling the latest options.
  // Updated post-commit (not during render), so it is one render behind -
  // never read `optionsRef.current` during render; only from event handlers
  // or effects (all callbacks below already follow this).
  const optionsRef = useRef(options);
  useEffect(() => {
    optionsRef.current = options;
  });

  const reload = useCallback(async () => {
    try {
      const data = await optionsRef.current.fetch();
      setItems(data.items);
      if (data.related) setRelated(data.related);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch on mount. The load is awaited inside an async wrapper so the state
  // updates land in a later task instead of synchronously inside the effect.
  useEffect(() => {
    void (async () => {
      await reload();
    })();
  }, [reload]);

  const query = searchQuery.toLowerCase();
  const filteredItems = items.filter((item) => {
    const text = options.searchText(item);
    const haystack = Array.isArray(text) ? text : [text];
    return haystack.some((value) => value.toLowerCase().includes(query));
  });

  const openModal = useCallback((item?: TItem) => {
    const { formName, fillForm } = optionsRef.current;
    setEditingId(item ? item.id : null);
    setName(item && formName ? formName(item) : "");
    fillForm?.(item ?? null);
    setIsFormOpen(true);
  }, []);

  const closeModal = useCallback(() => setIsFormOpen(false), []);

  const handleDelete = useCallback(
    async (item: TItem) => {
      const {
        remove,
        confirmDelete,
        deleteSuccessMessage,
        deleteErrorMessage = "Ошибка удаления",
      } = optionsRef.current;
      if (!remove) return;
      if (confirmDelete && !window.confirm(confirmDelete(item))) return;
      try {
        await remove(item.id);
        if (deleteSuccessMessage) toast.success(deleteSuccessMessage);
        await reload();
      } catch (e) {
        toast.error(getApiErrorMessage(e, deleteErrorMessage));
      }
    },
    [reload],
  );

  const handleRestore = useCallback(
    async (item: TItem) => {
      const {
        restore,
        restoreSuccessMessage,
        restoreErrorMessage = "Ошибка восстановления",
      } = optionsRef.current;
      if (!restore) return;
      try {
        await restore(item.id);
        await reload();
        if (restoreSuccessMessage) {
          toast.success(restoreSuccessMessage, { style: { color: "green" } });
        }
      } catch (e) {
        toast.error(getApiErrorMessage(e, restoreErrorMessage));
      }
    },
    [reload],
  );

  const handleSubmit = useCallback(
    async (event: React.FormEvent) => {
      event.preventDefault();
      const {
        validate,
        submit,
        saveErrorMessage = "Ошибка сохранения",
      } = optionsRef.current;
      if (validate && !validate()) return;
      setIsSubmitting(true);
      try {
        await submit(editingId);
        setIsFormOpen(false);
        await reload();
      } catch (e) {
        toast.error(getApiErrorMessage(e, saveErrorMessage));
      } finally {
        setIsSubmitting(false);
      }
    },
    [editingId, reload],
  );

  return {
    items,
    related,
    loading,
    reload,
    searchQuery,
    setSearchQuery,
    filteredItems,
    isFormOpen,
    closeModal,
    openModal,
    editingId,
    isSubmitting,
    name,
    setName,
    handleDelete,
    handleRestore,
    handleSubmit,
  };
};
