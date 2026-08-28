export interface UserPayload {
  sub: string;
  role: string;
  exp: number;
}

export const AUTH_LOGOUT_EVENT = "auth:logout";

// Clears the token and signals AuthSessionListener to navigate to /login
// client-side, so callers outside the router tree (e.g. the axios
// interceptor) never need a hard window.location navigation.
export const logout = (): void => {
  localStorage.removeItem("token");
  window.dispatchEvent(new Event(AUTH_LOGOUT_EVENT));
};

export const getUserInfo = (): UserPayload | null => {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join(""),
    );

    return JSON.parse(jsonPayload);
  } catch (e) {
    console.error("Invalid token", e);
    return null;
  }
};

export const isAdmin = (): boolean => {
  const user = getUserInfo();
  return user?.role === "Admin";
};
