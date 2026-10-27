import { Copy, Key, X } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage } from "@/api.ts";

export interface IssuedCredentials {
  login: string;
  pass: string;
  link: string;
}

interface CredentialsBannerProps {
  title: string;
  credentials: IssuedCredentials;
  onClose: () => void;
}

// Shows a freshly issued login/password once, with a one-click copy, so the
// admin can hand it over; the password is never retrievable afterwards.
export const CredentialsBanner = ({
  title,
  credentials,
  onClose,
}: CredentialsBannerProps) => {
  const handleCopy = async () => {
    const text = `Ссылка для входа: ${credentials.link}\nЛогин: ${credentials.login}\nПароль: ${credentials.pass}`;
    try {
      await navigator.clipboard.writeText(text);
      toast.success("Данные для входа скопированы");
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Не удалось скопировать"));
    }
  };

  return (
    <div className="bg-green-50 border border-green-200 dark:bg-green-950/30 dark:border-green-900 p-4 flex justify-between items-center animate-in slide-in-from-top-2">
      <div className="flex gap-4 items-center">
        <div className="w-10 h-10  bg-green-100 text-green-600 dark:bg-green-900/40 dark:text-green-400 flex items-center justify-center">
          <Key size={20} />
        </div>
        <div>
          <p className="text-green-900 dark:text-green-300 font-bold text-sm uppercase">
            {title}
          </p>
          <p className="text-sm text-green-700 dark:text-green-400 mt-1">
            Логин:{" "}
            <b className="font-mono bg-white/50 dark:bg-black/20 px-1 ">
              {credentials.login}
            </b>{" "}
            • Пароль:{" "}
            <b className="font-mono bg-white/50 dark:bg-black/20 px-1 ">
              {credentials.pass}
            </b>{" "}
            • Ссылка:{" "}
            <b className="font-mono bg-white/50 dark:bg-black/20 px-1 ">
              {credentials.link}
            </b>
          </p>
        </div>
      </div>
      <div className="flex items-center gap-1">
        <Button
          variant="ghost"
          size="icon"
          onClick={handleCopy}
          className="text-green-700 hover:bg-green-100 dark:text-green-400 dark:hover:bg-green-900/40"
          title="Скопировать логин, пароль и ссылку"
        >
          <Copy size={18} />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          onClick={onClose}
          className="text-green-700 hover:bg-green-100 dark:text-green-400 dark:hover:bg-green-900/40"
        >
          <X size={18} />
        </Button>
      </div>
    </div>
  );
};
