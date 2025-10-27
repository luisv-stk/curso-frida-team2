import React from 'react';
import { FaCamera, FaFilm } from 'react-icons/fa';

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

type ImageCardProps = {
  imageSrc: string;
  icon: React.ReactNode;
};

const ImageCard: React.FC<ImageCardProps> = ({ imageSrc, icon }) => (
  <div className="relative bg-black">
    <img src={imageSrc} alt="gallery" className="w-full h-full object-cover" />
    <div className="absolute top-2 right-2 text-white bg-black rounded-full p-1">{icon}</div>
  </div>
);

interface GalleryProps {
  publications: Publication[];
}

const Gallery: React.FC<GalleryProps> = ({ publications = [] }) => {
  const columns: Publication[][] = [[], [], []];
  publications.forEach((pub, index) => {
    columns[index % 3].push(pub);
  });

  return (
    <div className="min-h-screen text-white flex flex-col p-4">
      <nav className="flex justify-center space-x-4 p-4">
        {['Inicio', 'Fotos', 'Videos', 'Ilustraciones', '3D'].map((category) => (
          <button
            key={category}
            className="px-4 py-2 rounded-full bg-black text-white border border-white hover:bg-white hover:text-black"
          >
            {category}
          </button>
        ))}
      </nav>

      <div className="flex-grow flex justify-center items-start p-4">
        <div className="flex gap-4 w-full max-w-6xl">
          {columns.map((column, i) => (
            <div key={i} className="flex flex-col gap-4 flex-1">
              {column.map((pub, j) => (
                <ImageCard
                  key={`${i}-${j}`}
                  imageSrc={pub.imageUrl}
                  icon={pub.fileType === 'Vídeo' ? <FaFilm /> : <FaCamera />}
                />
              ))}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Gallery;
