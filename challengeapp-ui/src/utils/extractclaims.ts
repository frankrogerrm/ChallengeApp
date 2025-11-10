import { jwtDecode } from "jwt-decode";

export type UserClaims = {
  role: string;
  email: string;
};

export function extractClaims(token: string): UserClaims {
  const decoded = jwtDecode<Record<string, string>>(token);

  const roleClaim = Object.entries(decoded).find(([key]) =>
    key.toLowerCase().includes("role")
  );
  const emailClaim = Object.entries(decoded).find(([key]) =>
    key.toLowerCase().includes("email")
  );

  return {
    role: roleClaim?.[1] ?? "",
    email: emailClaim?.[1] ?? decoded.sub ?? "",
  };
}
