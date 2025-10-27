// src/app/models/login-response.model.ts

export interface Usuario {
  id: number;
  nombre: string;
  email: string;
  // agrega aquí los campos que devuelva tu backend
}

export interface LoginResponse {
  Status: string;          // coincide con el backend
  Token: string;
  ExpiracionToken: string; // puedes convertir a Date después
  Usuario: Usuario;        // aquí sí definimos la interfaz Usuario
}
