import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  getApiErrorMessage,
  type DictionaryItem,
} from "../api";
import { AdminLayout } from "../layouts/AdminLayout";
import { Plus, Edit2, Trash2, Building2, RotateCcw } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Separator } from "@/components/ui/separator";
import { AppSidebar } from "@/components/app-sidebar";

export const AdminDepartmentsPage = () => {
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [newName, setNewName] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const res = await dictionariesApi.getDepartments();
      setDepartments(res.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const openModal = (d?: DictionaryItem) => {
    if (d) {
      setEditingId(d.id);
      setNewName(d.name);
    } else {
      setEditingId(null);
      setNewName("");
    }
    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Удалить кафедру?")) return;
    try {
      await dictionariesApi.deleteDepartment(id);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreDepartment(id);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка восстановления"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) await dictionariesApi.updateDepartment(editingId, newName);
      else await dictionariesApi.createDepartment(newName);
      setIsFormOpen(false);
      loadData();
    } catch (e) {
      alert("Ошибка");
    }
  };

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center justify-between gap-2 px-4 border-b border-slate-100 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12 bg-white">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator
              orientation="vertical"
              className="mr-2 data-[orientation=vertical]:h-4"
            />
            <Breadcrumb>
              <BreadcrumbList>
                <BreadcrumbItem className="hidden md:block">
                  <BreadcrumbPage className="font-medium text-slate-500">
                    Справочники
                  </BreadcrumbPage>
                </BreadcrumbItem>
                <BreadcrumbSeparator className="hidden md:block" />
              </BreadcrumbList>
            </Breadcrumb>
          </div>
        </header>
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50/50 border-b border-slate-200">
                <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase">
                  Название / Аббревиатура
                </th>
                <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase text-right">
                  Действия
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {departments.map((d) => (
                <tr
                  key={d.id}
                  className={`group transition-colors ${
                    d.isDeleted
                      ? "bg-slate-50/70 text-slate-400"
                      : "hover:bg-slate-50"
                  }`}
                >
                  <td className="py-4 px-6">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-orange-50 text-orange-600">
                        <Building2 size={16} />
                      </div>
                      <span
                        className={`text-sm font-bold ${d.isDeleted ? "text-slate-500" : "text-slate-900"}`}
                      >
                        {d.name}
                      </span>
                      {d.isDeleted && (
                        <span className="inline-flex items-center rounded-full bg-slate-200 px-2 py-1 text-[10px] font-bold uppercase tracking-wide text-slate-600">
                          Удалено
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="py-4 px-6 text-right">
                    <div className="flex items-center justify-end gap-2 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
                      {d.isDeleted ? (
                        <Button
                          onClick={() => handleRestore(d.id)}
                          title="Восстановить"
                        >
                          <RotateCcw size={18} />
                        </Button>
                      ) : (
                        <>
                          <Button variant="ghost" onClick={() => openModal(d)}>
                            <Edit2 size={18} />
                          </Button>
                          <Button
                            variant="ghost"
                            onClick={() => handleDelete(d.id)}
                          >
                            <Trash2 size={18} />
                          </Button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {isFormOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
              onClick={() => setIsFormOpen(false)}
            ></div>
            <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6">
              <h3 className="text-lg font-bold text-slate-900 mb-4">
                {editingId ? "Редактирование" : "Новая кафедра"}
              </h3>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                    Название
                  </label>
                  <input
                    className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm"
                    value={newName}
                    onChange={(e) => setNewName(e.target.value)}
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <Button type="button" onClick={() => setIsFormOpen(false)}>
                    Отмена
                  </Button>
                  <Button type="submit">Сохранить</Button>
                </div>
              </form>
            </div>
          </div>
        )}
      </SidebarInset>
    </SidebarProvider>
  );
};
