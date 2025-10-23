import React, { useState } from 'react';
import { FaArrowLeft, FaCheck, FaCloudUploadAlt, FaCamera } from 'react-icons/fa';

interface FormData {
  fileType: string;
  title: string;
  price: string;
  uploadedFile: File | null;
}

interface AddNewItemProps {
  onBack: () => void;
}

const AddNewItem: React.FC<AddNewItemProps> = ({ onBack }) => {
  const [formData, setFormData] = useState<FormData>({
    fileType: 'Fotografía',
    title: '',
    price: '',
    uploadedFile: null,
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prevData => ({
      ...prevData,
      [name]: value,
    }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files ? e.target.files[0] : null;
    setFormData(prevData => ({
      ...prevData,
      uploadedFile: file,
    }));
  };

  const handleSubmit = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();

    // --- Logging the data ---
    console.groupCollapsed('Form Data Collected (Ready for API)');
    console.log('Tipo de Archivo (fileType):', formData.fileType);
    console.log('Título (title):', formData.title);
    console.log('Precio (price):', formData.price);

    if (formData.uploadedFile) {
        console.log('Archivo Cargado (uploadedFile):', {
            name: formData.uploadedFile.name,
            size: `${(formData.uploadedFile.size / 1024 / 1024).toFixed(2)} MB`,
            type: formData.uploadedFile.type,
        });
    } else {
        console.log('Archivo Cargado (uploadedFile): No se ha seleccionado ningún archivo.');
    }
    console.groupEnd();
  };

  const handleClear = () => {
    setFormData({
      fileType: 'Fotografía',
      title: '',
      price: '',
      uploadedFile: null,
    });
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
              className="text-[#0276ff]"
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
          {formData.uploadedFile ? `Archivo seleccionado: ${formData.uploadedFile.name}` : 'Arrastra desde tu ordenador el archivo que quieres cargar'}
        </p>
        
          {!formData.uploadedFile && (
              <p className="text-sm text-[#6f7274] mt-2">
                  Recuerda que el archivo no puede superar los 2 Mb. y tiene que ser en formato .JPG o .PNG
              </p>
          )}
      </div>

      <div className="mt-6 flex space-x-4">
        <button
          onClick={handleClear}
          className="py-2 px-4 border rounded-full bg-[#d9d9d9] text-black"
        >
          Borrar
        </button>
        <button
          onClick={handleSubmit}
          className="py-2 px-4 border rounded-full bg-[#0276ff] text-white font-semibold hover:bg-[#005bb5] transition-colors"
        >
          Cargar archivo
        </button>
      </div>
    </div>
  );
};

export default AddNewItem;