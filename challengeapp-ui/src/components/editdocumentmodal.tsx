import { useState } from "react";
import axios from "axios";

type TagDto = { name: string };
type DocumentDto = {
  id: number;
  title: string;
  accessType: string;
  uploadedBy: string;
  tags: TagDto[];
};

type Props = {
  token: string;
  document: DocumentDto;
  onClose: () => void;
  onSuccess: () => void;
};

export default function EditDocumentModal({
  token,
  document,
  onClose,
  onSuccess,
}: Props) {
  const [title, setTitle] = useState(document.title);
  const [accessType, setAccessType] = useState(document.accessType);
  const [tags, setTags] = useState<TagDto[]>(document.tags || []);
  const [newTag, setNewTag] = useState("");
  const [loading, setLoading] = useState(false);

  const apiBase = process.env.NEXT_PUBLIC_API_URL;

  const handleAddTag = () => {
    if (newTag.trim()) {
      setTags([...tags, { name: newTag.trim() }]);
      setNewTag("");
    }
  };

  const handleRemoveTag = (name: string) => {
    setTags(tags.filter((tag) => tag.name !== name));
  };

  const handleUpdate = async () => {
    setLoading(true);
    try {
      await axios.put(
        `${apiBase}/documents/${document.id}`,
        { title, accessType, tags },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      onSuccess();
    } catch {
      alert("Failed to update document.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      className="modal show d-block"
      style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
    >
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Edit Document</h5>
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
                <label htmlFor="title" className="form-label">
                  Title:
                </label>
                <input
                  type="text"
                  className="form-control"
                  id="title"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </div>

              <div className="mb-3">
                <label htmlFor="accessType" className="form-label">
                  Access Type:
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
                <label className="form-label">Tags:</label>
                <div className="input-group mb-3">
                  <input
                    type="text"
                    className="form-control"
                    value={newTag}
                    onChange={(e) => setNewTag(e.target.value)}
                    placeholder="Enter a tag"
                  />
                  <button
                    className="btn btn-outline-secondary"
                    type="button"
                    onClick={handleAddTag}
                  >
                    Add Tag
                  </button>
                </div>
                <div className="d-flex flex-wrap gap-2">
                  {tags.map((tag, i) => (
                    <span key={i} className="badge bg-secondary">
                      {tag.name}{" "}
                      <button
                        type="button"
                        className="btn-close btn-close-white"
                        style={{ fontSize: "0.65em" }}
                        onClick={() => handleRemoveTag(tag.name)}
                        aria-label="Remove tag"
                      ></button>
                    </span>
                  ))}
                </div>
              </div>
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
              onClick={handleUpdate}
              disabled={loading}
            >
              {loading ? (
                <>
                  <span
                    className="spinner-border spinner-border-sm me-2"
                    role="status"
                    aria-hidden="true"
                  ></span>
                  Saving...
                </>
              ) : (
                "Save Changes"
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
