// import React, { useState } from "react";
// import { useNavigate } from "react-router-dom";
// import toast from "react-hot-toast";
// import api from "../api";
// export default function Signup() {
//   const navigate = useNavigate();
//   const [form, setForm] = useState({
//     name: "",
//     email: "",
//     password: "",
//     DepartmentName: "",
//   });
//   const [errors, setErrors] = useState({});

//   const handleChange = (e) => {
//     setForm({ ...form, [e.target.name]: e.target.value });
//   };

//   // Validation function
//   const validateForm = () => {
//     const newErrors = {};

//     if (!form.name.trim()) newErrors.name = "Full name is required";
//     if (!form.email) newErrors.email = "Email is required";
//     else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
//       newErrors.email = "Invalid email format";

//     if (!form.password) newErrors.password = "Password is required";
//     else if (form.password.length < 6)
//       newErrors.password = "Password must be at least 6 characters";

//     if (!form.DepartmentName.trim())
//       newErrors.DepartmentName = "Department is required";
//     else if (!/^[A-Za-z\s]+$/.test(form.DepartmentName))
//       newErrors.DepartmentName = "Department name must contain only letters";

//     setErrors(newErrors);
//     return Object.keys(newErrors).length === 0; 
//   };

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     if (!validateForm()) {
//       toast.error("Please fix the highlighted errors.");
//       return;
//     }

//     try {
//       await api.post(`/auth/register`, form);
//       toast.success(" Account created successfully! Please login.");
//       navigate("/login");
//     } catch (err) {
//       console.error("Signup failed", err);
//       toast.error(" Signup failed. Try again.");
//     }
//   };

//   return (
//     <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
//       <div className="max-w-md w-full bg-white p-8 rounded-xl shadow-md">
//         <h2 className="text-2xl font-bold text-gray-900 text-center mb-6">
//           Create an Account
//         </h2>

//         <form onSubmit={handleSubmit} className="space-y-4">
//           {/* Full Name */}
//           <div>
//             <label className="block text-sm font-medium text-gray-700">
//               Full Name
//             </label>
//             <input
//               type="text"
//               name="name"
//               value={form.name}
//               onChange={handleChange}
//               className={`mt-1 block w-full rounded-md border ${
//                 errors.name ? "border-red-500" : "border-gray-300"
//               } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
//               placeholder="Jane Doe"
//             />
//             {errors.name && (
//               <p className="text-red-500 text-xs mt-1">{errors.name}</p>
//             )}
//           </div>

//           {/* Email */}
//           <div>
//             <label className="block text-sm font-medium text-gray-700">
//               Email address
//             </label>
//             <input
//               type="email"
//               name="email"
//               value={form.email}
//               onChange={handleChange}
//               className={`mt-1 block w-full rounded-md border ${
//                 errors.email ? "border-red-500" : "border-gray-300"
//               } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
//               placeholder="you@example.com"
//             />
//             {errors.email && (
//               <p className="text-red-500 text-xs mt-1">{errors.email}</p>
//             )}
//           </div>

//           {/* Password */}
//           <div>
//             <label className="block text-sm font-medium text-gray-700">
//               Password
//             </label>
//             <input
//               type="password"
//               name="password"
//               value={form.password}
//               onChange={handleChange}
//               className={`mt-1 block w-full rounded-md border ${
//                 errors.password ? "border-red-500" : "border-gray-300"
//               } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
//               placeholder="••••••••"
//             />
//             {errors.password && (
//               <p className="text-red-500 text-xs mt-1">{errors.password}</p>
//             )}
//           </div>

//           {/* Department */}
//           <div>
//             <label className="block text-sm font-medium text-gray-700">
//               Department
//             </label>
//             <input
//               type="text"
//               name="DepartmentName"
//               value={form.DepartmentName}
//               onChange={handleChange}
//               className={`mt-1 block w-full rounded-md border ${
//                 errors.DepartmentName ? "border-red-500" : "border-gray-300"
//               } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
//               placeholder="Enter your department"
//             />
//             {errors.DepartmentName && (
//               <p className="text-red-500 text-xs mt-1">
//                 {errors.DepartmentName}
//               </p>
//             )}
//           </div>

//           <button
//             type="submit"
//             className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 font-medium"
//           >
//             Signup
//           </button>
//         </form>

//         <p className="mt-6 text-center text-sm text-gray-600">
//           Already have an account?{" "}
//           <button
//             type="button"
//             onClick={() => navigate("/login")}
//             className="text-blue-600 hover:text-blue-700 font-medium"
//           >
//             Login
//           </button>
//         </p>
//       </div>
//     </div>
//   );
// }
// src/pages/Signup.jsx
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import api from "../api";
import FormValidator from "../utils/formValidation"; 

export default function Signup() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
    DepartmentName: "",
  });
  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    //  Use the reusable validator
    const validationErrors = FormValidator.validateSignup(form);
    setErrors(validationErrors);

    if (!FormValidator.isValid(validationErrors)) {
      toast.error("Please fix the highlighted errors.");
      return;
    }

    try {
      await api.post(`/auth/register`, form);
      toast.success("Account created successfully! Please login.");
      navigate("/login");
    } catch (err) {
      console.error("Signup failed", err);
      toast.error("Signup failed. Try again.");
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full bg-white p-8 rounded-xl shadow-md">
        <h2 className="text-2xl font-bold text-gray-900 text-center mb-6">
          Create an Account
        </h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Full Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Full Name
            </label>
            <input
              type="text"
              name="name"
              value={form.name}
              onChange={handleChange}
              className={`mt-1 block w-full rounded-md border ${
                errors.name ? "border-red-500" : "border-gray-300"
              } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
              placeholder="Jane Doe"
            />
            {errors.name && (
              <p className="text-red-500 text-xs mt-1">{errors.name}</p>
            )}
          </div>

          {/* Email */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Email address
            </label>
            <input
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              className={`mt-1 block w-full rounded-md border ${
                errors.email ? "border-red-500" : "border-gray-300"
              } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
              placeholder="you@example.com"
            />
            {errors.email && (
              <p className="text-red-500 text-xs mt-1">{errors.email}</p>
            )}
          </div>

          {/* Password */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Password
            </label>
            <input
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              className={`mt-1 block w-full rounded-md border ${
                errors.password ? "border-red-500" : "border-gray-300"
              } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
              placeholder="••••••••"
            />
            {errors.password && (
              <p className="text-red-500 text-xs mt-1">{errors.password}</p>
            )}
          </div>

          {/* Department */}
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Department
            </label>
            <input
              type="text"
              name="DepartmentName"
              value={form.DepartmentName}
              onChange={handleChange}
              className={`mt-1 block w-full rounded-md border ${
                errors.DepartmentName ? "border-red-500" : "border-gray-300"
              } shadow-sm focus:ring-blue-600 focus:border-blue-600 sm:text-sm p-2`}
              placeholder="Enter your department"
            />
            {errors.DepartmentName && (
              <p className="text-red-500 text-xs mt-1">
                {errors.DepartmentName}
              </p>
            )}
          </div>

          <button
            type="submit"
            className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 font-medium"
          >
            Signup
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-600">
          Already have an account?{" "}
          <button
            type="button"
            onClick={() => navigate("/login")}
            className="text-blue-600 hover:text-blue-700 font-medium"
          >
            Login
          </button>
        </p>
      </div>
    </div>
  );
}
