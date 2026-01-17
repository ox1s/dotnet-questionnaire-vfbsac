import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api, { type Form } from "../api";
import { LogOut, FileText } from "lucide-react";

export const DashboardPage = () => {
  const [forms, setForms] = useState<Form[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    api
      .get<Form[]>("/forms")
      .then((res) => setForms(res.data))
      .catch(() => navigate("/login"));
  }, [navigate]);

  const logout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm px-6 py-4 flex justify-between items-center">
        <div className="flex items-center gap-6">
          <h1 className="text-xl font-bold text-blue-900">Личный кабинет</h1>
          <Link
            to="/admin/create-form"
            className="text-sm bg-blue-100 text-blue-700 px-3 py-1.5 rounded-md font-medium hover:bg-blue-200 transition"
          >
            + Создать анкету
          </Link>
        </div>

        <button
          onClick={logout}
          className="text-gray-500 hover:text-red-600 flex gap-2 items-center"
        >
          <LogOut size={18} /> Выход
        </button>
      </nav>

      <main className="max-w-4xl mx-auto mt-8 px-4">
        <h2 className="text-2xl font-semibold mb-6">Доступные анкеты</h2>
        <div className="grid gap-4 md:grid-cols-2">
          {forms.map((form) => (
            <div key={form.id} className="block group">
              <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 hover:border-blue-500 transition-colors">
                <div className="flex items-start justify-between">
                  <div className="flex gap-3">
                    <div className="p-2 bg-blue-50 rounded-lg text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                      <FileText size={24} />
                    </div>
                    <div>
                      <h3 className="font-medium text-lg">{form.title}</h3>
                      <p className="text-sm text-gray-500 mt-1">
                        Требует:{" "}
                        {form.requiredFilters?.join(", ") || "Нет фильтров"}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="mt-4 flex gap-2 border-t pt-4 border-gray-100">
                  <Link
                    to={`/form/${form.id}`}
                    className="text-sm text-blue-600 hover:underline font-medium"
                  >
                    Пройти анкету
                  </Link>

                  <Link
                    to={`/admin/stats/${form.id}`}
                    className="text-sm text-gray-500 hover:text-gray-900 hover:underline ml-auto"
                  >
                    Статистика (Admin)
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
};
