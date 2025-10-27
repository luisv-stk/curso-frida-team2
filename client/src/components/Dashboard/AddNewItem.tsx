import React, { useState, useEffect } from 'react';
import { FaArrowLeft, FaCheck, FaCloudUploadAlt, FaCamera } from 'react-icons/fa';

export type Publication = {
  name: string;
  fileType: string;
  size: string;
  description: string;
  price: string;
  author: string;
  date: string;
  imageUrl: string;
};

interface FormData {
  fileType: string;
  title: string;
  price: string;
  uploadedFile: File | null;
}

interface AddNewItemProps {
  onBack: () => void;
  onSubmitSuccess: (newPub: Publication) => void;
}

const AddNewItem: React.FC<AddNewItemProps> = ({ onBack, onSubmitSuccess }) => {
  useEffect(() => {
    if (!onSubmitSuccess || typeof onSubmitSuccess !== 'function') {
      throw new Error('onSubmitSuccess prop is missing or not a function');
    }
  }, [onSubmitSuccess]);

  const [formData, setFormData] = useState<FormData>({
    fileType: 'Fotografía',
    title: '',
    price: '',
    uploadedFile: null,
  });

  const [isUploading, setIsUploading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    setFormData(prev => ({ ...prev, uploadedFile: file }));
  };

  const handleSubmit = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    if (!formData.uploadedFile) return alert('Selecciona un archivo primero.');

    setIsUploading(true);

    try {
      const body = new FormData();
      body.append('image', formData.uploadedFile);

      const response = await fetch('http://localhost:5203/api/ImageTagging/generate-tags', {
        method: 'POST',
        body,
      });

      if (!response.ok) throw new Error('Error al subir el archivo.');

      const result = await response.json();

      const newPublication: Publication = {
        name: formData.title || formData.uploadedFile.name,
        fileType: formData.fileType,
        size: `${(formData.uploadedFile.size / 1024).toFixed(1)} KB`,
        description: result.tags || 'Sin descripción generada',
        price: formData.price || 'Gratis',
        author: 'Tú',
        date: new Date().toLocaleDateString('es-ES'),
        imageUrl: URL.createObjectURL(formData.uploadedFile),
      };

      onSubmitSuccess(newPublication);
    } catch (error) {
      console.error(error);
      alert('Hubo un error al subir el archivo.');
    } finally {
      setIsUploading(false);
    }
  };

  const UploadIcon = formData.uploadedFile ? FaCamera : FaCloudUploadAlt;

  return (
    <div className="mt-16 bg-[#f5f9fd] p-10">
      <div className="flex justify-between items-start">
        <div className="flex items-center text-[#0276ff]">
          <FaArrowLeft className="mr-2 cursor-pointer" onClick={onBack} />
          <h2 className="text-xl font-bold">Añadir nuevo elemento</h2>
        </div>
        <button className="text-[#0276ff] flex items-center">
          <FaCheck className="mr-2" />
          Cerrar
        </button>
      </div>

      <div className="mt-6">
        <h3 className="text-lg font-bold">Datos del archivo</h3>
      </div>

      <div className="mt-4 flex space-x-4">
        {['Fotografía', 'Vídeo', 'Ilustración', '3D'].map(type => (
          <label key={type} className="flex items-center space-x-2">
            <input
              type="radio"
              name="fileType"
              value={type}
              checked={formData.fileType === type}
              onChange={handleChange}
            />
            <span>{type}</span>
          </label>
        ))}
      </div>

      <div className="mt-6 grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm">Título</label>
          <input
            type="text"
            name="title"
            value={formData.title}
            onChange={handleChange}
            className="w-full border rounded text-sm p-2 border-[#e4e4e4] bg-white focus:outline-none focus:ring-1 focus:ring-[#0276ff]"
          />
        </div>
        <div>
          <label className="block text-sm">Precio</label>
          <input
            type="text"
            name="price"
            value={formData.price}
            onChange={handleChange}
            className="w-full border rounded text-sm p-2 border-[#e4e4e4] bg-white focus:outline-none focus:ring-1 focus:ring-[#0276ff]"
          />
        </div>
      </div>

      <div className="mt-4 border-dashed border-2 border-[#0276ff] bg-white rounded p-6 flex flex-col items-center text-center relative">
        <input
          type="file"
          name="uploadedFile"
          onChange={handleFileChange}
          className="absolute opacity-0 w-full h-full cursor-pointer"
          style={{ zIndex: 10, top: 0, left: 0 }}
          accept=".jpg,.png"
        />

        <UploadIcon className="text-[#ee28ff] text-4xl" />

        <p className="mt-4 font-semibold">
          {formData.uploadedFile
            ? `Archivo seleccionado: ${formData.uploadedFile.name}`
            : 'Arrastra desde tu ordenador el archivo que quieres cargar'}
        </p>

        {!formData.uploadedFile && (
          <p className="text-sm text-[#6f7274] mt-2">
            Recuerda que el archivo no puede superar los 2 Mb. y tiene que ser en formato .JPG o .PNG
          </p>
        )}
      </div>

      <div className="mt-6 flex space-x-4">
        <button
          onClick={() =>
            setFormData({
              fileType: 'Fotografía',
              title: '',
              price: '',
              uploadedFile: null,
            })
          }
          className="py-2 px-4 border rounded-full bg-[#d9d9d9] text-black"
        >
          Borrar
        </button>
        <button
          onClick={handleSubmit}
          disabled={isUploading}
          className="py-2 px-4 border rounded-full bg-[#0276ff] text-white font-semibold hover:bg-[#005bb5] transition-colors disabled:opacity-50"
        >
          {isUploading ? 'Subiendo...' : 'Cargar archivo'}
        </button>
      </div>
    </div>
  );
};

export default AddNewItem;
