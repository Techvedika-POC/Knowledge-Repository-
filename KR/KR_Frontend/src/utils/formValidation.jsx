export default class FormValidator {

  static validateSignup(form) {
    const errors = {};

    if (!form.name?.trim()) errors.name = "Full name is required";
    if (!form.email) errors.email = "Email is required";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      errors.email = "Invalid email format";

    if (!form.password) errors.password = "Password is required";
    else if (form.password.length < 6)
      errors.password = "Password must be at least 6 characters";

    if (!form.DepartmentName?.trim())
      errors.DepartmentName = "Department is required";
    else if (!/^[A-Za-z\s]+$/.test(form.DepartmentName))
      errors.DepartmentName = "Department name must contain only letters";

    return errors;
  }

  // Validate Knowledge Item Upload form
  static validateKnowledgeItem(form, files) {
    const errors = {};

    if (!form.name?.trim())
      errors.name = "Knowledge item name is required";

    if (!form.domainId)
      errors.domainId = "Please select a domain";

    if (!form.categoryId)
      errors.categoryId = "Please select a category";

    if (!form.description?.trim())
      errors.description = "Description is required";

    // Validate languages if present
    if (form.languages && typeof form.languages === "string") {
      const langs = form.languages
        .split(",")
        .map((l) => l.trim())
        .filter(Boolean);
      if (langs.length === 0)
        errors.languages = "At least one language must be specified";
    }

    // Optional: Ensure at least one file if File upload tab is active
    if (files?.length === 0)
      errors.files = "Please upload at least one file";

    return errors;
  }

  // Utility to check if form is valid
  static isValid(errors) {
    return Object.keys(errors).length === 0;
  }
}
