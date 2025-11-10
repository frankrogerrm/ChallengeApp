import { useState } from "react";
import axios from "axios";

type TagDto = { Name: string };

type Props = {
  token: string;
  onClose: () => void;
  onSuccess: () => void;
};

export default function UploadModal({ token, onClose, onSuccess }: Props) {
  const [file, setFile] = useState<File | null>(null);
  const [title, setTitle] = useState("");
  const [accessType, setAccessType] = useState("Public");
  const [tags, setTags] = useState<TagDto[]>([]);
  const [newTag, setNewTag] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const userEmail =
    typeof window !== "undefined" ? localStorage.getItem("userEmail") : "";

  const handleAddTag = () => {
    if (newTag.trim()) {
      setTags([...tags, { Name: newTag.trim() }]);
      setNewTag("");
    }
  };

  const handleRemoveTag = (name: string) => {
    setTags(tags.filter((tag) => tag.Name !== name));
  };

  const handleUpload = async () => {
    if (!file || !title || !accessType || !token || !userEmail) {
      setError("Missing required fields.");
      return;
    }

    const ext = file.name.split(".").pop()?.toLowerCase();
    const sizeMB = file.size / 1024 / 1024;
    if (!["pdf", "docx", "txt"].includes(ext || "")) {
      setError("Unsupported file type.");
      return;
    }
    if (sizeMB > 10) {
      setError("File exceeds 10 MB limit.");
      return;
    }

    const formData = new FormData();
    formData.append("file", file);
    formData.append("title", title);
    formData.append("accessType", accessType);
    formData.append("tags", JSON.stringify(tags));

    try {
      const url = `${process.env.NEXT_PUBLIC_API_URL}/documents/upload`;
      await axios.post(url, formData, {
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "multipart/form-data",
        },
      });

      setSuccess("File uploaded successfully.");
      setError("");
      onClose();
      onSuccess();
    } catch {
      setError("Failed to upload file.");
      setSuccess("");
    }
  };

  return (
    <div
      className="modal show d-block"
      style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
    >
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Upload Document</h5>
            <button
              type="button"
              className="btn-close"
              onClick={onClose}
              aria-label="Close"
            ></button>
          </div>
          <div className="modal-body">
            <form>
              <div className="mb-3">
                <label htmlFor="documentTitle" className="form-label">
                  Title
                </label>
                <input
                  type="text"
                  className="form-control"
                  id="documentTitle"
                  placeholder="Document title"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </div>

              <div className="mb-3">
                <label htmlFor="accessType" className="form-label">
                  Access Type
                </label>
                <select
                  className="form-select"
                  id="accessType"
                  value={accessType}
                  onChange={(e) => setAccessType(e.target.value)}
                >
                  <option value="Public">Public</option>
                  <option value="Private">Private</option>
                  <option value="Restricted">Restricted</option>
                </select>
              </div>

              <div className="mb-3">
                <label className="form-label">Tags</label>
                <div className="input-group mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Add tag"
                    value={newTag}
                    onChange={(e) => setNewTag(e.target.value)}
                  />
                  <button
                    className="btn btn-outline-secondary"
                    type="button"
                    onClick={handleAddTag}
                  >
                    Add
                  </button>
                </div>
                <div className="d-flex flex-wrap gap-2">
                  {tags.map((tag, i) => (
                    <span key={i} className="badge bg-secondary">
                      {tag.Name}{" "}
                      <button
                        type="button"
                        className="btn-close btn-close-white"
                        style={{ fontSize: "0.65em" }}
                        onClick={() => handleRemoveTag(tag.Name)}
                        aria-label="Remove tag"
                      ></button>
                    </span>
                  ))}
                </div>
              </div>

              <div className="mb-3">
                <label htmlFor="fileUpload" className="form-label">
                  Document File
                </label>
                <input
                  type="file"
                  className="form-control"
                  id="fileUpload"
                  onChange={(e) => setFile(e.target.files?.[0] || null)}
                  accept=".pdf,.docx,.txt"
                />
              </div>

              {error && (
                <div className="alert alert-danger" role="alert">
                  {error}
                </div>
              )}
              {success && (
                <div className="alert alert-success" role="alert">
                  {success}
                </div>
              )}
            </form>
          </div>
          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
            >
              Cancel
            </button>
            <button
              type="button"
              className="btn btn-primary"
              onClick={handleUpload}
              disabled={!file || !title}
            >
              Upload
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
