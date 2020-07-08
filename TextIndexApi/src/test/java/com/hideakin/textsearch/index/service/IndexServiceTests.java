package com.hideakin.textsearch.index.service;

import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.ArrayList;
import java.util.Arrays;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.TextRepository;

@SpringBootTest
public class IndexServiceTests {

	@Mock
	private FileGroupRepository fileGroupRepository;

	@Mock
	private FileRepository fileRepository;

	@Mock
	private TextRepository textRepository;

	@InjectMocks
	private IndexService indexService = new IndexServiceImpl();

	@Test
	public void findText_nogroup() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(null);
		TextDistribution[] hits = indexService.findText("xyzzy", "FOO", SearchOptions.Exact);
		Assertions.assertEquals(0, hits.length);
		verify(fileGroupRepository, times(1)).findByName("xyzzy");
	}

	@Test
	public void findText_nohit() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(3, "xyzzy"));
		when(textRepository.findByTextAndGid("FOO", 3)).thenReturn(null);
		TextDistribution[] hits = indexService.findText("xyzzy", "FOO", SearchOptions.Exact);
		Assertions.assertEquals(0, hits.length);
		verify(fileGroupRepository, times(1)).findByName("xyzzy");
		verify(textRepository, times(1)).findByTextAndGid("FOO", 3);
		verify(textRepository, times(0)).findAllByTextContainingAndGid("FOO", 3);
		verify(textRepository, times(0)).findAllByTextStartingWithAndGid("FOO", 3);
		verify(textRepository, times(0)).findAllByTextEndingWithAndGid("FOO", 3);
	}

	@Test
	public void findText_exact() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(4, "xyzzy"));
		when(textRepository.findByTextAndGid("FOO", 4)).thenReturn(
				new TextEntity("FOO", 4, new byte[] { 3, 1, 11, 5, 2, 13, 17, 7, 3, 19, 23, 29 }));
		when(fileRepository.findByFid(3)).thenReturn(new FileEntity(3, "quux.cpp", 1024, 4));
		when(fileRepository.findByFid(5)).thenReturn(new FileEntity(5, "fred.cs", 2048, 4));
		when(fileRepository.findByFid(7)).thenReturn(new FileEntity(7, "waldo.java", 8192, 4));
		TextDistribution[] hits = indexService.findText("xyzzy", "FOO", SearchOptions.Exact);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("xyzzy");
		verify(textRepository, times(1)).findByTextAndGid("FOO", 4);
		verify(textRepository, times(0)).findAllByTextContainingAndGid("FOO", 4);
		verify(textRepository, times(0)).findAllByTextStartingWithAndGid("FOO", 4);
		verify(textRepository, times(0)).findAllByTextEndingWithAndGid("FOO", 4);
		verify(fileRepository, times(1)).findByFid(3);
		verify(fileRepository, times(1)).findByFid(5);
		verify(fileRepository, times(1)).findByFid(7);
	}

	@Test
	public void findText_contains() {
		when(fileGroupRepository.findByName("corge")).thenReturn(new FileGroupEntity(4, "corge"));
		when(textRepository.findAllByTextContainingAndGid("BAR", 4)).thenReturn(
				new ArrayList<TextEntity>(Arrays.asList(
						new TextEntity("XBARX", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("YBARY", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				)));
		when(fileRepository.findByFid(3)).thenReturn(new FileEntity(3, "quux.cpp", 1024, 4));
		when(fileRepository.findByFid(5)).thenReturn(new FileEntity(5, "fred.cs", 2048, 4));
		when(fileRepository.findByFid(7)).thenReturn(new FileEntity(7, "waldo.java", 8192, 4));
		TextDistribution[] hits = indexService.findText("corge", "BAR", SearchOptions.Contains);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("corge");
		verify(textRepository, times(0)).findByTextAndGid("BAR", 4);
		verify(textRepository, times(1)).findAllByTextContainingAndGid("BAR", 4);
		verify(textRepository, times(0)).findAllByTextStartingWithAndGid("BAR", 4);
		verify(textRepository, times(0)).findAllByTextEndingWithAndGid("BAR", 4);
		verify(fileRepository, times(1)).findByFid(3);
		verify(fileRepository, times(1)).findByFid(5);
		verify(fileRepository, times(1)).findByFid(7);
	}

	@Test
	public void findText_startsWith() {
		when(fileGroupRepository.findByName("corge")).thenReturn(new FileGroupEntity(4, "corge"));
		when(textRepository.findAllByTextStartingWithAndGid("BAZ", 4)).thenReturn(
				new ArrayList<TextEntity>(Arrays.asList(
						new TextEntity("BAZX", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("BAZY", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				)));
		when(fileRepository.findByFid(3)).thenReturn(new FileEntity(3, "quux.cpp", 1024, 4));
		when(fileRepository.findByFid(5)).thenReturn(new FileEntity(5, "fred.cs", 2048, 4));
		when(fileRepository.findByFid(7)).thenReturn(new FileEntity(7, "waldo.java", 8192, 4));
		TextDistribution[] hits = indexService.findText("corge", "BAZ", SearchOptions.StartsWith);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("corge");
		verify(textRepository, times(0)).findByTextAndGid("BAZ", 4);
		verify(textRepository, times(0)).findAllByTextContainingAndGid("BAZ", 4);
		verify(textRepository, times(1)).findAllByTextStartingWithAndGid("BAZ", 4);
		verify(textRepository, times(0)).findAllByTextEndingWithAndGid("BAZ", 4);
		verify(fileRepository, times(1)).findByFid(3);
		verify(fileRepository, times(1)).findByFid(5);
		verify(fileRepository, times(1)).findByFid(7);
	}

	@Test
	public void findText_endsWith() {
		when(fileGroupRepository.findByName("corge")).thenReturn(new FileGroupEntity(4, "corge"));
		when(textRepository.findAllByTextEndingWithAndGid("THUD", 4)).thenReturn(
				new ArrayList<TextEntity>(Arrays.asList(
						new TextEntity("XTHUD", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("YTHUD", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				)));
		when(fileRepository.findByFid(3)).thenReturn(new FileEntity(3, "quux.cpp", 1024, 4));
		when(fileRepository.findByFid(5)).thenReturn(new FileEntity(5, "fred.cs", 2048, 4));
		when(fileRepository.findByFid(7)).thenReturn(new FileEntity(7, "waldo.java", 8192, 4));
		TextDistribution[] hits = indexService.findText("corge", "THUD", SearchOptions.EndsWith);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("corge");
		verify(textRepository, times(0)).findByTextAndGid("THUD", 4);
		verify(textRepository, times(0)).findAllByTextContainingAndGid("THUD", 4);
		verify(textRepository, times(0)).findAllByTextStartingWithAndGid("THUD", 4);
		verify(textRepository, times(1)).findAllByTextEndingWithAndGid("THUD", 4);
		verify(fileRepository, times(1)).findByFid(3);
		verify(fileRepository, times(1)).findByFid(5);
		verify(fileRepository, times(1)).findByFid(7);
	}

}
