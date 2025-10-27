import React, { useState } from 'react';
import { FaArrowLeft } from 'react-icons/fa';
import AddNewItem from './AddNewItem';
import ImageCard from '../ImageCard';

export interface Publication {
  name: string;
  fileType: string;
  size: string;
  description: string;
  price: string;
  author: string;
  date: string;
  imageUrl: string;
}

interface PublicationsPageProps {
  publications: Publication[];
  onAddNew: (pub: Publication) => void;
  onBack: () => void;
}

const PublicationsPage: React.FC<PublicationsPageProps> = ({ publications, onAddNew, onBack }) => {
  const [showAddNew, setShowAddNew] = useState(false);

  if (showAddNew) {
    return <AddNewItem onBack={() => setShowAddNew(false)} onSubmitSuccess={onAddNew} />;
  }

  return (
    <div className="w-screen min-h-screen bg-[#f5f9fd] p-8">
      <div className="flex justify-between items-center p-4">
        <div className="flex items-center text-[#0276ff] cursor-pointer mt-8" onClick={onBack}>
          <FaArrowLeft className="mr-2" />
          <h2 className="text-2xl font-bold">Todas mis publicaciones</h2>
        </div>
        <button className="text-[#0276ff] text-xl" onClick={() => setShowAddNew(true)}>
          Añadir nuevo +
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-4">
        {publications.length === 0 ? (
          <p className="text-center text-gray-500 col-span-full">No hay publicaciones aún.</p>
        ) : (
          publications.map((pub, index) => (
            <ImageCard
              key={index}
              imageSrc={pub.imageUrl}
              name={pub.name}
              price={pub.price}
              fileType={pub.fileType}
              author={pub.author}
              dateUploaded={pub.date}
              size={pub.size}
              tags={pub.description}
            />
          ))
        )}
      </div>
    </div>
  );
};

export default PublicationsPage;
