package com.hideakin.textsearch.index.service;

import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.EntityManager;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.repository.FileContentRepository;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;

@SpringBootTest
public class FileServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileRepository fileRepository;

	@Mock
	private FileGroupRepository fileGroupRepository;

	@Mock
	private FileContentRepository fileContentRepository;

	@InjectMocks
	private FileService fileService = new FileServiceImpl();

	@Test
	public void getFiles_successful() {
		when(fileGroupRepository.findByName("quux")).thenReturn(new FileGroupEntity(333, "quux"));
		List<FileEntity> entities = new ArrayList<FileEntity>();
		add(entities, 801, "/home/src/quux/foo.java", 333);
		add(entities, 802, "/home/src/quux/bar.java", 333);
		add(entities, 803, "/home/src/quux/baz.java", 333);
		when(fileRepository.findAllByGidAndStaleFalse(333)).thenReturn(entities);
		FileInfo[] fi = fileService.getFiles("quux");
		Assertions.assertEquals(3, fi.length);
		Assertions.assertEquals(entities.get(0).getPath(), fi[0].getPath());
		Assertions.assertEquals(entities.get(1).getPath(), fi[1].getPath());
		Assertions.assertEquals(entities.get(2).getPath(), fi[2].getPath());
	}

	@Test
	public void getFiles_nogroup() {
		when(fileGroupRepository.findByName("quux")).thenReturn(null);
		FileInfo[] fi = fileService.getFiles("quux");
		Assertions.assertEquals(null, fi);
	}

	@Test
	public void getFiles_null() {
		when(fileGroupRepository.findByName("quux")).thenReturn(new FileGroupEntity(333, "quux"));
		when(fileRepository.findAllByGid(333)).thenReturn(null);
		FileInfo[] fi = fileService.getFiles("quux");
		Assertions.assertEquals(0, fi.length);
	}

	@Test
	public void getPath_successful() {
		when(fileRepository.findByFid(456)).thenReturn(new FileEntity(456, "/home/plugh/thud.cs", 1000, 789));
		String path = fileService.getPath(456);
		Assertions.assertEquals("/home/plugh/thud.cs", path);
	}

	@Test
	public void getPath_null() {
		when(fileRepository.findByFid(456)).thenReturn(null);
		String path = fileService.getPath(456);
		Assertions.assertEquals(null, path);
	}

	@Test
	public void deleteFiles_successful() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(111, "xyzzy"));
		List<FileEntity> entities = new ArrayList<FileEntity>();
		add(entities, 801, "/home/src/quux/foo.java", 111);
		add(entities, 802, "/home/src/quux/bar.java", 111);
		add(entities, 803, "/home/src/quux/baz.java", 111);
		when(fileRepository.findAllByGid(111)).thenReturn(entities);
		doNothing().when(fileContentRepository).deleteByFid(801);
		doNothing().when(fileContentRepository).deleteByFid(802);
		doNothing().when(fileContentRepository).deleteByFid(803);
		doNothing().when(fileRepository).deleteByGid(111);
		when(em.createQuery("SELECT text FROM texts")).thenReturn(new PseudoQuery(new ArrayList<String>()));
		FileInfo[] fi = fileService.deleteFiles("xyzzy");
		Assertions.assertEquals(801, fi[0].getFid());
		Assertions.assertEquals(802, fi[1].getFid());
		Assertions.assertEquals(803, fi[2].getFid());
		verify(fileContentRepository, times(1)).deleteByFid(801);
		verify(fileContentRepository, times(1)).deleteByFid(802);
		verify(fileContentRepository, times(1)).deleteByFid(803);
		verify(em, times(1)).createQuery("SELECT text FROM texts");
		verify(fileRepository, times(1)).deleteByGid(111);
	}

	private void add(List<FileEntity> entities, int fid, String path, int gid) {
		entities.add(new FileEntity(fid, path, 1000, gid));
	}

}
