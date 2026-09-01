import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { AUTH_LOGOUT_EVENT } from "@/utils/auth";

// Mounted inside BrowserRouter so any logout() caller (including code
// outside the router tree, like the axios interceptor) can trigger a
// client-side redirect to /login without a full-page navigation.
export const AuthSessionListener = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const handleLogout = () => navigate("/login", { replace: true });
    window.addEventListener(AUTH_LOGOUT_EVENT, handleLogout);
    return () => window.removeEventListener(AUTH_LOGOUT_EVENT, handleLogout);
  }, [navigate]);

  return null;
};
