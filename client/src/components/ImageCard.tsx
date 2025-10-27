import React from 'react';
import { FaEdit, FaTrash } from 'react-icons/fa';

interface ImageCardProps {
  imageSrc: string;
  name: string;
  price: string;
  fileType: string;
  author: string;
  dateUploaded: string;
  size: string;
  tags: string;
}

const ImageCard: React.FC<ImageCardProps> = ({
  imageSrc,
  name,
  price,
  fileType,
  author,
  dateUploaded,
  size,
  tags,
}) => {

const tagList = (tags || '')
  .toString()
  .split(',')
  .map(tag => tag.trim())
  .filter(tag => tag)
  .slice(0, 10);


  return (
    <div className="w-full max-w-4xl flex bg-white p-4 rounded shadow">
      <div className="flex-shrink-0 relative">
        <img
          src={imageSrc}
          alt={name}
          className="w-48 h-48 object-cover rounded"
        />
        <div className="absolute bottom-2 left-2 flex space-x-2">
          <button className="bg-gray-200 p-2 rounded-full">
            <FaEdit className="text-gray-800" />
          </button>
          <button className="bg-gray-200 p-2 rounded-full">
            <FaTrash className="text-gray-800" />
          </button>
        </div>
      </div>

      <div className="ml-6 flex flex-col justify-between flex-1 overflow-hidden">
        <div className="flex flex-wrap justify-between gap-2">
          <span className="font-medium text-gray-900 truncate">Nombre: {name}</span>
          <span className="font-medium text-gray-900 truncate">Precio: {price}</span>
        </div>

        <div className="flex flex-wrap justify-between gap-2 mt-1">
          <span className="text-gray-900 truncate">Tipo de archivo: {fileType}</span>
          <span className="text-gray-900 truncate">Autor: {author}</span>
        </div>

        <div className="flex flex-wrap justify-between gap-2 mt-1">
          <span className="text-gray-900 truncate">Tamaño: {size}</span>
          <span className="text-gray-900 truncate">Fecha subida: {dateUploaded}</span>
        </div>

        <hr className="my-2 border-gray-300" />

        <div className="flex flex-wrap gap-2">
          {tagList.length === 0 ? (
            <span className="text-gray-500 text-sm">Sin etiquetas</span>
          ) : (
            tagList.map((tag, index) => (
              <span
                key={index}
                className="bg-purple-100 text-purple-800 text-xs font-medium px-2 py-1 rounded-full"
              >
                {tag}
              </span>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default ImageCard;
