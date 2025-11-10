import { useEffect, useState } from "react";
import axios from "axios";

type Props = {
  token: string;
  documentId: number;
  onClose: () => void;
};

type DocumentDetails = {
  id: number;
  title: string;
  accessType: string;
  fileName: string;
  fileSize: number;
  uploadedBy: string;
  createdDate: string;
  modifiedDate: string;
  tags: string[];
};

export default function ViewDocumentDetailsModal({
  token,
  documentId,
  onClose,
}: Props) {
  const [details, setDetails] = useState<DocumentDetails | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchDetails = async () => {
      try {
        const url = `${process.env.NEXT_PUBLIC_API_URL}/documents/${documentId}`;
        const res = await axios.get<DocumentDetails>(url, {
          headers: { Authorization: `Bearer ${token}` },
        });
        setDetails(res.data);
      } catch {
        setError("Failed to load document details.");
      }
    };

    fetchDetails();
  }, [token, documentId]);

  return (
    <div
      className="modal show d-block"
      style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
    >
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Document Details</h5>
            <button type="button" className="btn-close" onClick={onClose} />
          </div>
          <div className="modal-body">
            {error ? (
              <div className="alert alert-danger">{error}</div>
            ) : details ? (
              <div>
                <p>
                  <strong>Title:</strong> {details.title}
                </p>
                <p>
                  <strong>Access Type:</strong> {details.accessType}
                </p>
                <p>
                  <strong>File Name:</strong> {details.fileName}
                </p>
                <p>
                  <strong>File Size:</strong>{" "}
                  {(details.fileSize / 1024).toFixed(2)} KB
                </p>
                <p>
                  <strong>Uploaded By:</strong> {details.uploadedBy}
                </p>
                <p>
                  <strong>Created:</strong>{" "}
                  {new Date(details.createdDate).toLocaleString()}
                </p>
                <p>
                  <strong>Last Modified:</strong>{" "}
                  {new Date(details.modifiedDate).toLocaleString()}
                </p>
                <p>
                  <strong>Tags:</strong>{" "}
                  {details.tags.length > 0 ? details.tags.join(", ") : "—"}
                </p>
              </div>
            ) : (
              <p>Loading...</p>
            )}
          </div>
          <div className="modal-footer">
            <button className="btn btn-secondary" onClick={onClose}>
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
