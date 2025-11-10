type LoginResponse = {
  token: string;
  user: {
    email: string;
    role: string;
  };
};