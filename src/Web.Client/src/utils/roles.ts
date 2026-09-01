// Mirrors Domain.User.UserRole (excluding Admin, which never fills out forms).
// Used to label a form's target audience (plural/collective phrasing).
export const ROLE_LABELS: Record<string, string> = {
  StudentGroup: "Студенты",
  Staff: "Сотрудники",
  DeputyHead: "Заместитель декана",
  Employer: "Наниматели",
};

// Used to label the signed-in user's own role (singular phrasing).
export const OWN_ROLE_LABELS: Record<string, string> = {
  StudentGroup: "Учащийся",
  Staff: "Сотрудник",
  DeputyHead: "Заместитель декана",
  Employer: "Наниматель",
};
