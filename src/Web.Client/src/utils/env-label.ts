// Marks non-production deploys. Set VITE_ENV_LABEL (e.g. "ТЕСТ") in the
// build environment of a test deploy; leave it unset on production.
export const getEnvLabel = (): string =>
  (import.meta.env.VITE_ENV_LABEL ?? "").trim();

export const withEnvLabel = (title: string): string => {
  const label = getEnvLabel();

  return label ? `[${label}] ${title}` : title;
};
