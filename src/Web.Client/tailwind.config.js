/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        // Цвета из макета (Admin Dashboard)
        primary: "#1a4456", // Глубокий синий
        "primary-hover": "#133342",
        secondary: "#5b7c8b", // Серо-голубой
        "background-light": "#f9fafb",
        "surface-light": "#ffffff",
        "surface-dark": "#1e2b39",
      },
      fontFamily: {
        display: ["Manrope", "sans-serif"],
        body: ["Manrope", "sans-serif"],
      },
      borderRadius: {
        xl: "0.75rem",
        "2xl": "1rem",
      },
    },
  },
  plugins: [],
};
