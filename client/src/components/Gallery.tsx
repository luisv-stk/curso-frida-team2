import React from 'react';
import { FaCamera, FaFilm } from 'react-icons/fa';

type ImageCardProps = {
  imageSrc: string;
  icon: React.ReactNode;
};

const ImageCard: React.FC<ImageCardProps> = ({ imageSrc, icon }) => {
  return (
    <div className="relative bg-black">
      <img src={imageSrc} alt="gallery" className="w-full h-full object-cover" />
      <div className="absolute top-2 right-2 text-white bg-black rounded-full p-1">{icon}</div>
    </div>
  );
};

const Gallery: React.FC = () => {
  const categories = ["Inicio", "Fotos", "Videos", "Ilustraciones", "3D"];
  const imageSources = [
    ["https://placehold.co/400x400/jpg", "https://placehold.co/400x400/jpg"],
    ["https://placehold.co/400x400/jpg", "https://placehold.co/400x400/jpg"],
    ["https://placehold.co/400x400/jpg", "https://placehold.co/400x400/jpg"]
  ];
  
  const iconList = [[<FaCamera/>, <FaCamera/>],
                    [<FaFilm/>, <FaCamera/>],
                    [<FaCamera/>, <FaFilm/>]];

  return (
<div className="min-h-screen bg-black text-white flex flex-col">
  <nav className="flex justify-center space-x-4 p-4">
    {categories.map((category) => (
      <button
        key={category}
        className="px-4 py-2 rounded-full bg-black text-white border border-white hover:bg-white hover:text-black"
      >
        {category}
      </button>
    ))}
  </nav>

  <div className="flex-grow flex justify-center items-start p-4">
    <div className="grid grid-cols-3 gap-4">
      {imageSources.map((column, i) => (
        <div key={i} className="flex flex-col gap-4">
          {column.map((src, j) => (
            <ImageCard key={src} imageSrc={src} icon={iconList[i][j]} />
          ))}
        </div>
      ))}
    </div>
  </div>
</div>
  );
}

export default Gallery;