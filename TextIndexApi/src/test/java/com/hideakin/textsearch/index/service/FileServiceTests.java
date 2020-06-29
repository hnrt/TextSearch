package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.ArgumentMatchers.any;
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
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@SpringBootTest
public class FileServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupService fileGroupService;

	@Mock
	private FileRepository fileRepository;

	@Mock
	private PreferenceRepository preferenceRepository;

	@InjectMocks
	private FileService fileService = new FileServiceImpl();

	@Test
	public void getFiles_successful() {
		when(fileGroupService.getGid("quux")).thenReturn(333);
		List<FileEntity> entities = new ArrayList<FileEntity>();
		add(entities, 801, "/home/src/quux/foo.java", 333);
		add(entities, 802, "/home/src/quux/bar.java", 333);
		add(entities, 803, "/home/src/quux/baz.java", 333);
		when(fileRepository.findAllByGid(333)).thenReturn(entities);
		String[] paths = fileService.getFiles("quux");
		Assertions.assertEquals(3, paths.length);
		Assertions.assertEquals(entities.get(0).getPath(), paths[0]);
		Assertions.assertEquals(entities.get(1).getPath(), paths[1]);
		Assertions.assertEquals(entities.get(2).getPath(), paths[2]);
	}

	@Test
	public void getFiles_nogroup() {
		when(fileGroupService.getGid("quux")).thenReturn(-1);
		String[] paths = fileService.getFiles("quux");
		Assertions.assertEquals(null, paths);
	}

	@Test
	public void getFiles_null() {
		when(fileGroupService.getGid("quux")).thenReturn(444);
		when(fileRepository.findAllByGid(444)).thenReturn(null);
		String[] paths = fileService.getFiles("quux");
		Assertions.assertEquals(0, paths.length);
	}

	@Test
	public void getFid_successful() {
		List<FileEntity> entities = new ArrayList<FileEntity>();
		add(entities, 801, "/var/lib/xyzzy/helloworld.java", 554);
		add(entities, 802, "/var/lib/xyzzy/helloworld.java", 555);
		add(entities, 803, "/var/lib/xyzzy/helloworld.java", 556);
		when(fileRepository.findAllByPath("/var/lib/xyzzy/helloworld.java")).thenReturn(entities);
		int fid = fileService.getFid("/var/lib/xyzzy/helloworld.java", 555);
		Assertions.assertEquals(802, fid);
	}

	@Test
	public void getFid_nogroup() {
		List<FileEntity> entities = new ArrayList<FileEntity>();
		add(entities, 901, "/var/lib/xyzzy/helloworld.java", 667);
		add(entities, 902, "/var/lib/xyzzy/helloworld.java", 668);
		add(entities, 903, "/var/lib/xyzzy/helloworld.java", 669);
		when(fileRepository.findAllByPath("/var/lib/xyzzy/helloworld.java")).thenReturn(entities);
		int fid = fileService.getFid("/var/lib/xyzzy/helloworld.java", 666);
		Assertions.assertEquals(-1, fid);
	}

	@Test
	public void getFid_null() {
		when(fileRepository.findAllByPath("/var/lib/xyzzy/helloworld.java")).thenReturn(null);
		int fid = fileService.getFid("/var/lib/xyzzy/helloworld.java", 777);
		Assertions.assertEquals(-1, fid);
	}

	@Test
	public void getFids_successful() {
		List<FileEntity> entities = new ArrayList<FileEntity>();
		add(entities, 901, "/var/lib/xyzzy/fred1.java", 888);
		add(entities, 902, "/var/lib/xyzzy/fred2.java", 888);
		add(entities, 903, "/var/lib/xyzzy/fred3.java", 888);
		when(fileRepository.findAllByGid(888)).thenReturn(entities);
		List<Integer> fids = fileService.getFids(888);
		Assertions.assertEquals(3, fids.size());
		Assertions.assertEquals(901, fids.get(0).intValue());
		Assertions.assertEquals(902, fids.get(1).intValue());
		Assertions.assertEquals(903, fids.get(2).intValue());
	}

	@Test
	public void getFids_null() {
		when(fileRepository.findAllByGid(888)).thenReturn(null);
		List<Integer> fids = fileService.getFids(888);
		Assertions.assertEquals(null, fids);
	}

	@Test
	public void addFile_successful() {
		when(preferenceRepository.findByName("FID.next")).thenReturn(null);
		when(em.createQuery("SELECT MAX(fid) FROM files")).thenReturn(new PseudoQuery(987));
		when(preferenceRepository.save(argThat(x -> x.getName().equals("FID.next")))).thenReturn(new PreferenceEntity("FID.next", "989"));
		when(fileRepository.save(argThat(x -> x.getFid() == 988))).thenReturn(new FileEntity(988, "/home/waldo/corge.h", 123));
		int fid = fileService.addFile("/home/waldo/corge.h", 123);
		Assertions.assertEquals(988, fid);
		verify(preferenceRepository, times(1)).findByName("FID.next");
		verify(em, times(1)).createQuery("SELECT MAX(fid) FROM files");
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("FID.next") && x.getValue().equals("989")));
		verify(fileRepository, times(1)).save(argThat(x -> x.getFid() == 988));
	}

	@Test
	public void getPath_successful() {
		when(fileRepository.findByFid(456)).thenReturn(new FileEntity(456, "/home/plugh/thud.cs", 789));
		String path = fileService.getPath(456, 789);
		Assertions.assertEquals("/home/plugh/thud.cs", path);
	}

	@Test
	public void getPath_groupunmatched() {
		when(fileRepository.findByFid(456)).thenReturn(new FileEntity(456, "/home/plugh/thud.cs", 789));
		String path = fileService.getPath(456, 790);
		Assertions.assertEquals(null, path);
	}

	@Test
	public void getPath_null() {
		when(fileRepository.findByFid(456)).thenReturn(null);
		String path = fileService.getPath(456, 789);
		Assertions.assertEquals(null, path);
	}

	@Test
	public void delete_successful() {
		List<Integer> fids = new ArrayList<Integer>();
		fids.add(11);
		fids.add(12);
		fids.add(13);
		fids.add(14);
		fids.add(15);
		fileService.delete(fids);
		verify(fileRepository, times(1)).deleteById(11);
		verify(fileRepository, times(1)).deleteById(12);
		verify(fileRepository, times(1)).deleteById(13);
		verify(fileRepository, times(1)).deleteById(14);
		verify(fileRepository, times(1)).deleteById(15);
	}

	@Test
	public void delete_empty() {
		List<Integer> fids = new ArrayList<Integer>();
		fileService.delete(fids);
		verify(fileRepository, times(0)).deleteById(any(int.class));
	}

	private void add(List<FileEntity> entities, int fid, String path, int gid) {
		entities.add(new FileEntity(fid, path, gid));
	}

}
